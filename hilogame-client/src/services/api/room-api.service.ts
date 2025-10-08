import {inject, Injectable} from '@angular/core';
import {firstValueFrom, lastValueFrom} from 'rxjs';
import {ERoomStatus, Room, RoomLeaderboard} from '../../app/models/room.model';
import {ApiService} from './api.service';
import {PageResult} from '../../app/models/generic/pagination.model';

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

  async getRooms(status: ERoomStatus, page = 1, pageSize = 10): Promise<PageResult<Room>> {
    return await firstValueFrom(this.api.get<PageResult<Room>>('rooms', {
      status,
      page,
      pageSize,
    }));
  }

  async getLeaderboard(roomId: string): Promise<RoomLeaderboard> {
    return await firstValueFrom<RoomLeaderboard>(this.api.get<RoomLeaderboard>(`rooms/${roomId}/leaderboard`));
  }

}
