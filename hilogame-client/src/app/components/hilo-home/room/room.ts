import {Component, computed, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {AuthService} from '../../../../services/auth.service';
import {RoomLeaderboardComponent} from '../room-leaderboard/room-leaderboard';
import {NgClass, UpperCasePipe} from '@angular/common';
import {RoomService} from './room.service';
import {Room} from '../../../models/room.model';

@Component({
  selector: 'app-room',
  imports: [
    FormsModule,
    RoomLeaderboardComponent,
    UpperCasePipe,
    NgClass
  ],
  templateUrl: './room.html',
  styleUrl: './room.scss'
})
export class RoomComponent {

  // Injection
  public roomService = inject(RoomService);
  public authService = inject(AuthService);

  // Signals
  public player = this.authService.player;
  public players = computed(() => this.room().roomPlayers.map(p => p.username));
  public guess = signal<number>(0);
  public lastFeedback = this.roomService.lastFeedback;

  //Functions
  public isEliminated = (n: number) => n <= this.lowerBound() || n >= this.upperBound();

  // Computed
  public room = computed<Room>(() => {
    return this.roomService.currentRoom()!;
  })

  public score = computed(() => {
    let fb = this.roomService.lastFeedback();
    if (fb) {
      return fb.attemptsAfterThisGuess;
    }
    return 0;
  })

  public lowerBound = computed(() => {
    let room = this.room();
    const fb = this.lastFeedback();
    return fb ? fb.secretIsBiggerThan : room.min - 1;  // éliminés: n <= lowerBound
  });

  public upperBound = computed(() => {
    let room = this.room();
    const fb = this.lastFeedback();
    return fb ? fb.secretIsLessThan : room.max + 1;      // éliminés: n >= upperBound
  });

  public numbers = computed(() => {

    let room = this.room();

    let min = room.min;
    let max = room.max;

    let numbers = [];
    for (let i = min; i <= max; i++) {
      numbers.push(i);
    }
    return numbers;
  })

  //Actions
  async startGame() {
    await this.roomService.startGame(this.room().id);
  }
  async makeGuess(guess: number) {
    await this.roomService.makeGuess(this.room().id, guess);
  }
  async leaveGame() {
    await this.roomService.leaveRoom(this.room().id);
  }
}
