import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {AuthTokens} from '../../app/models/auth.model';
import {firstValueFrom, lastValueFrom} from 'rxjs';
import {Room, RoomLeaderboard} from '../../app/models/room.model';
import {ApiService} from './api.service';

@Injectable({
  providedIn: 'root'
})
export class RoomApiService {

  private api = inject(ApiService);

  public async createRoom(roomName: string) {
    return await lastValueFrom<boolean>(this.api.post<boolean>("rooms", {
      Name: roomName,
    }))
  }

  async getPendingRooms() {
    return await firstValueFrom(this.api.get<Room[]>("rooms"));
  }

  async getLeaderboard(roomId: string): Promise<RoomLeaderboard> {
    return await firstValueFrom<RoomLeaderboard>(this.api.get<RoomLeaderboard>(`rooms/${roomId}/leaderboard`));
  }

}
