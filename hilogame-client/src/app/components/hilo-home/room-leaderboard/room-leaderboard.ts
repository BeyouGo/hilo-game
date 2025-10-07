import {Component, inject, OnInit, signal} from '@angular/core';
import {RoomService} from '../room/room.service';
import {RoomLeaderboard} from '../../../models/room.model';
import {DatePipe, NgClass} from '@angular/common';
import {RoomApiService} from '../../../../services/api/room-api.service';


@Component({
  selector: 'app-room-leaderboard',
  standalone: true,
  imports: [
    NgClass,
    DatePipe
  ],
  templateUrl: './room-leaderboard.html',
  styleUrl: './room-leaderboard.scss'
})
export class RoomLeaderboardComponent implements OnInit {

  //Injections
  private roomService = inject(RoomService);
  private roomApiService = inject(RoomApiService);

  //Signals
  protected room = this.roomService.currentRoom;
  protected leaderboard = signal<RoomLeaderboard | undefined>(undefined);

  async ngOnInit(): Promise<void> {
    const raw = await this.roomApiService.getLeaderboard(this.room()!.id);
    this.leaderboard.set(this.normalize(raw));
  }

  private normalize(raw: any): RoomLeaderboard {
    return {
      ...raw,
      finishedAt: new Date(raw.finishedAt),
      leaderboard: (raw.leaderboard ?? []).map((e: any) => ({
        ...e,
        firstGuessAt: e.firstGuessAt ? new Date(e.firstGuessAt) : null,
        lastGuessAt: e.lastGuessAt ? new Date(e.lastGuessAt) : null
      }))
    };
  }
}
