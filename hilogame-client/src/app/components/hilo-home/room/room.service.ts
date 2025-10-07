// import {effect, inject, Injectable, signal} from '@angular/core';
// import {HubConnectionBuilder, LogLevel} from '@microsoft/signalr';
// import {AuthService} from '../../../../services/auth.service';
// import {GuessFeedback, Room} from '../../../models/room.model';
// import {ToastService} from '../../../../services/toast.service';
// import {ApiService} from '../../../../services/api/api.service';
//
// @Injectable({
//   providedIn: 'root'
// })
// export class RoomService {
//
//   // Injections
//   private apiService = inject(ApiService);
//   private authService = inject(AuthService);
//   private toastService = inject(ToastService);
//
//   //Signals
//   private player = this.authService.player;
//   public currentRoom = signal<Room | undefined>(undefined);
//   public lastFeedback = signal<GuessFeedback | undefined>(undefined);
//
//
//   //Fields
//   private connection = new HubConnectionBuilder()
//     .withUrl(this.apiService.baseUrl + "/hubs/game", {
//       accessTokenFactory: () => this.authService.tokens()?.accessToken ?? "no-token",
//       transport: 1 /* WebSockets */
//     })
//     .withAutomaticReconnect()  // smart retries
//     .configureLogging(LogLevel.Information)
//     .build();
//
//   public constructor() {
//     effect(async () => {
//       let p = this.player();
//       await this.reset();
//     });
//     this.registerHubCall();
//   }
//
//   public registerHubCall() {
//
//     this.connection.on("PlayerJoined", (playerId, room: Room) => {
//       this.currentRoom.set(room);
//     });
//
//     this.connection.on("GuessFeedback", (roomId: string, playerId: string, feedback: GuessFeedback) => {
//       this.processGuessFeedback(feedback);
//     });
//
//     this.connection.on("PlayerDisconnected", (playerId: string, room: Room) => {
//       this.currentRoom.set(room)
//     });
//
//     this.connection.on("PlayerLeft", (playerId: string, room: Room) => {
//       this.currentRoom.set(room)
//     });
//
//
//     this.connection.on("GameStarted", (roomId: string) => {
//       this.currentRoom.update(room => {
//         if (!room) {
//           return undefined;
//         }
//
//         return {
//           ...room,
//           status: "started",
//         }
//       });
//     });
//
//     this.connection.on("RoomClosed", async (roomId: string, room: Room) => {
//       this.currentRoom.update(room => {
//
//         if (!room) {
//           return undefined;
//         }
//
//         return {
//           ...room,
//           status: "closed",
//         }
//       });
//
//       this.toastService.warning("The room has been closed");
//       await this.reset();
//     });
//
//     this.connection.on("GameEnded", (roomId: string) => {
//       this.currentRoom.update(room => {
//
//         if (!room) {
//           return undefined;
//         }
//
//         return {
//           ...room,
//           status: "finished",
//         }
//       });
//     });
//
//     this.connection.on("Error", (message) => console.error(message));
//   }
//
//
//   public async reset() {
//     await this.connection.stop();
//     this.lastFeedback.set(undefined);
//     this.currentRoom.set(undefined);
//   }
//
//   public async connect(room: Room) {
//
//     await this.reset()
//
//     await this.connection.start();
//     await this.connection.invoke("JoinRoom", room.id);
//   }
//
//   public async leaveRoom(roomId: string) {
//     await this.connection.invoke("LeaveRoom", roomId);
//     await this.reset();
//   }
//
//   public async startGame(roomId: string) {
//     await this.connection.invoke("StartGame", roomId);
//   }
//
//
//   public async makeGuess(roomId: string, value: number) {
//     await this.connection.invoke("MakeGuess", roomId, value);
//   }
//   private processGuessFeedback(feedback: GuessFeedback) {
//     this.lastFeedback.set(feedback);
//   }
//
//   // if we want to shut down a running game
//   // public async endGame(roomId: string) {
//   //   await this.connection.invoke("EndGame", roomId);
//   // }
// }


// room.service.ts
import {effect, inject, Injectable, signal, DestroyRef} from '@angular/core';
import {HubConnection, HubConnectionBuilder, HubConnectionState, HttpTransportType, LogLevel} from '@microsoft/signalr';
import {AuthService} from '../../../../services/auth.service';
import {GuessFeedback, Room} from '../../../models/room.model';
import {ToastService} from '../../../../services/toast.service';
import {ApiService} from '../../../../services/api/api.service';

type ConnState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({providedIn: 'root'})
export class RoomService {
  private apiService = inject(ApiService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  // state
  player = this.auth.player;
  currentRoom = signal<Room | undefined>(undefined);
  lastFeedback = signal<GuessFeedback | undefined>(undefined);
  connectionState = signal<ConnState>('disconnected');

  private connection: HubConnection = new HubConnectionBuilder()
    .withUrl(`${this.apiService.baseUrl}/hubs/game`, {
      accessTokenFactory: () => this.auth.tokens()?.accessToken ?? '',
      transport: HttpTransportType.WebSockets, // or omit to allow fallbacks
    })
    .withAutomaticReconnect([0, 1000, 2000, 5000, 10000])
    .configureLogging(LogLevel.Information)
    .build();

  constructor() {
    this.registerHubHandlers();

    const ref = effect(() => {
      // react to player changes: reset session
      const _ = this.player(); // track
      this.reset().catch(() => {
      });
    });
    this.destroyRef.onDestroy(() => ref.destroy());

    // lifecycle hooks
    this.connection.onreconnecting(() => this.connectionState.set('reconnecting'));
    this.connection.onreconnected(async () => {
      this.connectionState.set('connected');
      const room = this.currentRoom();
      // re-join SignalR group after reconnect
      if (room) {
        try {
          await this.connection.invoke('JoinRoom', room.id);
        } catch {
          this.toast.warning('Reconnected but failed to rejoin the room.');
        }
      }
    });
    this.connection.onclose(() => {
      this.connectionState.set('disconnected');
      // keep currentRoom to allow manual reconnect; or call reset() if you prefer
    });
  }

  private registerHubHandlers() {
    this.connection.on('PlayerJoined', (_playerId: string, room: Room) => this.currentRoom.set(room));
    this.connection.on('PlayerLeft', (_playerId: string, room: Room) => this.currentRoom.set(room));
    this.connection.on('PlayerDisconnected', (_playerId: string, room: Room) => this.currentRoom.set(room));

    this.connection.on('GuessFeedback', (_roomId: string, _playerId: string, fb: GuessFeedback) => {
      this.processGuessFeedback(fb);
    });

    this.connection.on('GameStarted', (_roomId: string) => {
      this.currentRoom.update(r => r ? {...r, status: 'started'} as Room : r);
    });

    this.connection.on('GameEnded', (_roomId: string) => {
      this.currentRoom.update(r => r ? {...r, status: 'finished'} as Room : r);
    });

    this.connection.on('RoomClosed', async (_roomId: string, _room: Room) => {
      this.currentRoom.update(r => r ? {...r, status: 'closed'} as Room : r);
      this.toast.warning('The room has been closed');
      await this.reset();
    });

    this.connection.on('Error', (message: string) => this.toast.error(message));
  }

  private async ensureConnected() {
    if (this.connection.state === HubConnectionState.Connected) return;
    if (this.connection.state === HubConnectionState.Connecting) {
      // small wait loop to avoid start() racing
      await new Promise(res => setTimeout(res, 100));
    }
    this.connectionState.set('connecting');
    await this.connection.start();
    this.connection.serverTimeoutInMilliseconds = 30_000;
    this.connection.keepAliveIntervalInMilliseconds = 10_000;
    this.connectionState.set('connected');
  }

  async reset() {
    try {
      await this.connection.stop();
    } catch {
    }
    this.lastFeedback.set(undefined);
    this.currentRoom.set(undefined);
    this.connectionState.set('disconnected');
  }

  async connect(room: Room) {
    // avoid needless reset if already in this room
    if (this.currentRoom()?.id !== room.id) {
      await this.reset();
    }
    try {
      await this.ensureConnected();
      this.currentRoom.set(room);
      await this.connection.invoke('JoinRoom', room.id);
    } catch (e) {
      this.toast.error('Failed to join the room.');
      throw e;
    }
  }

  async leaveRoom(roomId: string) {
    try {
      if (this.connection.state === HubConnectionState.Connected) {
        await this.connection.invoke('LeaveRoom', roomId);
      }
    } catch (e) {
      this.toast.warning('Problem leaving the room (will disconnect anyway).');
    } finally {
      await this.reset();
    }
  }

  async startGame(roomId: string) {
    try {
      await this.ensureConnected();
      await this.connection.invoke('StartGame', roomId);
    } catch {
      this.toast.error('Unable to start the game.');
    }
  }

  async makeGuess(roomId: string, value: number) {
    // optional: client-side guard by currentRoom rules
    try {
      await this.ensureConnected();
      await this.connection.invoke('MakeGuess', roomId, value);
    } catch {
      this.toast.error('Guess failed to send.');
    }
  }

  private processGuessFeedback(feedback: GuessFeedback) {
    this.lastFeedback.set(feedback);
  }
}
