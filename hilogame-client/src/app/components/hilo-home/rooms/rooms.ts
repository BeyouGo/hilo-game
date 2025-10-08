import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { RoomService } from '../room/room.service';
import { AuthService } from '../../../../services/auth.service';
import {ERoomStatus, Room} from '../../../models/room.model';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RoomApiService } from '../../../../services/api/room-api.service';
import {TitleCasePipe} from '@angular/common';

@Component({
  selector: 'app-rooms',
  imports: [ReactiveFormsModule, FormsModule, TitleCasePipe],
  templateUrl: './rooms.html',
  styleUrl: './rooms.scss',
  standalone: true
})
export class RoomsComponent implements OnInit {

  // Injections
  public roomService = inject(RoomService);
  public roomApiService = inject(RoomApiService);
  public authService = inject(AuthService);

  public player = this.authService.player;

  // data
  public rooms = signal<Room[]>([]);
  public selectedRoom = signal<Room | undefined>(undefined);

  // form
  protected roomName = signal<string>('');

  // pagination state
  page = signal(1);
  pageSize = signal(10);
  total = signal(0);
  loading = signal(false);

  // Filters
  statusFilter = signal<ERoomStatus>('awaitingPlayers');
  statuses: ReadonlyArray<ERoomStatus> = ['awaitingPlayers','started','finished','closed'] as const;


  totalPages = computed(() => {
    const t = this.total();
    const ps = this.pageSize();
    return Math.max(1, Math.ceil(t / ps));
  });

  fromIndex = computed(() => (this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1));
  toIndex = computed(() => Math.min(this.page() * this.pageSize(), this.total()));

  async ngOnInit() {
    await this.loadRooms();
  }


  async onStatusChange(newStatus: ERoomStatus) {
    this.statusFilter.set(newStatus);
    this.page.set(1);
    await this.loadRooms();
  }

  async loadRooms() {
    this.loading.set(true);
    try {
      const res = await this.roomApiService.getRooms(this.statusFilter(), this.page(), this.pageSize());
      this.rooms.set(res.items);
      this.total.set(res.total);
      this.page.set(res.page);
      this.pageSize.set(res.pageSize);
    } finally {
      this.loading.set(false);
    }
  }

  async selectRoom(room: Room) {
    this.selectedRoom.set(room);
    await this.roomService.connect(room);
  }

  async createRoom() {
    await this.roomApiService.createRoom(this.roomName());
    // after creation, reload page 1 to show newest rooms first (optional)
    this.page.set(1);
    await this.loadRooms();
    this.roomName.set('');
  }

  canCreateRoom() {
    return (this.roomName()?.trim()?.length ?? 0) > 0;
  }

  // pagination actions
  async goTo(p: number) {
    const clamped = Math.max(1, Math.min(p, this.totalPages()));
    if (clamped !== this.page()) {
      this.page.set(clamped);
      await this.loadRooms();
    }
  }

  async next() {
    await this.goTo(this.page() + 1);
  }

  async prev() {
    await this.goTo(this.page() - 1);
  }
}
