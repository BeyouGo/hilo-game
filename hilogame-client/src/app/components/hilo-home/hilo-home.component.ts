import {Component, inject} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {RoomComponent} from "./room/room";
import {RoomsComponent} from "./rooms/rooms";
import {RoomService} from './room/room.service';

@Component({
  selector: 'app-hilo-home.component',
    imports: [
        FormsModule,
        RoomComponent,
        RoomsComponent
    ],
  templateUrl: './hilo-home.component.html',
  styleUrl: './hilo-home.component.scss'
})
export class HiloHomeComponent {
  private roomService = inject(RoomService);
  protected readonly currentRoom = this.roomService.currentRoom;
}
