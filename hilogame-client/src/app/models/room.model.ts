export declare type ERoomStatus = "awaitingPlayers" | "started" | "finished" | "closed";

export interface RoomLeaderboard {
  roomId: string;
  winnerPlayerId: string;
  winnerUsername: string;
  finishedAt: Date;
  leaderboard: RoomLeaderboardEntry[];
}

export interface RoomLeaderboardEntry {
  playerId: string;
  playerUsername: string;
  finished: boolean;
  attempts: number;
  firstGuessAt: Date | null;
  lastGuessAt: Date | null;
  lowerBound: number;
  upperBound: number;
  currentSearchSpace: number;
  durationMs: number | null;
}


export interface Room {
  id: string;
  name: string;
  min: number;
  max: number;
  maxPlayerCount: number;
  ownerId: string;
  ownerUsername: string;
  createdAt: Date;
  status: ERoomStatus;
  winnerPlayer: string;
  playerCount: number;
  roomPlayers: { id: string; username: string }[];
}

export interface GuessFeedback {
  guess: number;
  result: "TooLow" | "TooHigh" | "Win";
  attemptsAfterThisGuess: number;
  secretIsBiggerThan: number;
  secretIsLessThan: number;
  finished: boolean;
}
