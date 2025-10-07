import {Component, inject, OnInit, signal} from '@angular/core';
import {RoomService} from '../room/room.service';
import {AuthService} from '../../../../services/auth.service';
import {Room} from '../../../models/room.model';
import {Router} from '@angular/router';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RoomApiService} from '../../../../services/api/room-api.service';

@Component({
  selector: 'app-rooms',
  imports: [
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './rooms.html',
  styleUrl: './rooms.scss'
})
export class RoomsComponent implements OnInit{

  public roomService = inject(RoomService);
  public roomApiService = inject(RoomApiService);
  public authService = inject(AuthService);

  public player = this.authService.player;

  public rooms = signal<Room[]>([]);
  public selectedRoom = signal<Room | undefined>(undefined);
  protected roomName = signal<string>("");

  async ngOnInit() {
    await this.loadRooms();
  }

  async loadRooms() {
    let rooms = await this.roomApiService.getPendingRooms();
    this.rooms.set(rooms);
  }

  async selectRoom(room: Room) {
    this.selectedRoom.set(room);
    await this.roomService.connect(room);
  }


  async createRoom() {
    await this.roomApiService.createRoom(this.roomName());
    this.loadRooms();
  }

  canCreateRoom() {
    return this.roomName()?.trim()?.length > 0;
  }
}
