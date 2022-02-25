using BombPeliLib;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BombPeliLib.Events;

namespace BombBot
{
	/// <summary>
	/// Interaction logic for Game.xaml
	/// </summary>
	public class Game
	{

		public delegate void PassBombEventHandler (object  sender, EventArgs e);
		public delegate void LeaveGameEventHandler (object sender, EventArgs e);

		public event PassBombEventHandler?  PassBomb;
		public event LeaveGameEventHandler? LeaveGame;

		private          GameState       gameState;
		private          Config          config;
		private          EventWaitHandle gameTask;
		readonly private object          guiLock = new object ();

		public Game (Config config, P2PApi client, GameInfo gi, int bombTime) {
			this.config    = config;
			this.gameState = new GameState (this.config, client, gi, bombTime);
			this.gameTask  = new EventWaitHandle (false, EventResetMode.ManualReset);
		}

		public GameState getState () {
			return this.gameState;
		}

		private void passbomb_Click (object sender, RoutedEventArgs e) {
			this.PassBomb?.Invoke (this, e);
		}

		private void quit_Click (object sender, RoutedEventArgs e) {
			this.LeaveGame?.Invoke (this, e);
		}

		public void init (P2PApi client) {
			this.LeaveGame        += this.LeaveGameHandler;
			this.PassBomb         += this.PassBombHandler;
			client.PeerLeft       += this.gameState.PeerLeftHandler;
			client.PeerLost       += this.PeerLostGameHandler;
			client.BombReceived   += this.BombReceivedHandler;
			client.SendBombFailed += this.SendBombFailedHandler;
		}

		public void release (P2PApi client) {
			client.PeerLeft       -= this.gameState.PeerLeftHandler;
			client.PeerLost       -= this.PeerLostGameHandler;
			client.BombReceived   -= this.BombReceivedHandler;
			client.SendBombFailed -= this.SendBombFailedHandler;
		}

		public bool start () {
			try {
				this.gameState.StartGame ();
			} catch (Exception ex) {
				Console.WriteLine ("Failed to start game.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
				return false;
			}
			Thread.Sleep (123);
			this.PassBomb?.Invoke (this, null);
			return true;
		}

		public void waitForGameEnd () {
			this.gameTask.WaitOne ();
		}

		public void LeaveGameHandler (object? sender, EventArgs e) {
			this.GameLogic (GameAction.LEAVE);
		}

		public void PeerLostGameHandler (object? sender, EventArgs e) {
			this.GameLogic (GameAction.PEER_LOST);
		}

		public void BombReceivedHandler (object? sender, BombReceivedEventArgs e) {
			this.GameLogic (GameAction.BOMB_RECEIVED, e);
		}

		public void PassBombHandler (object? sender, EventArgs e) {
			this.GameLogic (GameAction.PASS_BOMB);
		}

		public void SendBombFailedHandler (object? sender, EventArgs e) {
			this.GameLogic (GameAction.PASS_BOMB_FAIL);
		}

		public void WinGameHandler (object? sender, EventArgs e) {
			this.GameLogic (GameAction.WIN);
		}
		
		private void GameLogic (GameAction action, EventArgs? e = null) {
			switch (action) {
				case GameAction.BOMB_RECEIVED:  this.BombReceiveRoutine (this.gameState, e);	break;
				case GameAction.PASS_BOMB:      this.PassBombRoutine (this.gameState);			break;
                case GameAction.PASS_BOMB_FAIL: this.BombSendFailRoutine (this.gameState);		break;
                case GameAction.WIN:            this.WinGameRoutine (this.gameState);			break;
                case GameAction.LOSE:           this.LoseGameRoutine (this.gameState);			break;
                case GameAction.LEAVE:          this.LeaveGameRoutine (this.gameState);			break;
                case GameAction.PEER_LOST:      this.PeerLostGameRoutine (this.gameState);		break;
			}
        }

		private void LoseGameRoutine(GameState state) {
			state.Lose ();
			Console.WriteLine ("You lost");
			this.gameTask.Set ();
		}

		private void WinGameRoutine (GameState state) {
			state.Win ();
			Console.WriteLine ("Victory achieved.");
			this.gameTask.Set ();
		}

		private void LeaveGameRoutine (GameState state) {
			try {
				PeerInfo? peer = state.Client.GetRandomPeer ();
				if (peer.HasValue) {
					state.PassBomb (peer.Value, state.getRemainingBombTime ());
				}
				state.LeaveGame ();
			} catch (Exception ex) {
				Console.WriteLine ("Could not leave game.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
				return;
			}
			this.gameTask.Set ();
		}

		private void PeerLostGameRoutine (GameState state) {
			if (state.IsWinner ()) {
				this.GameLogic (GameAction.WIN);
			} else {
				state.ResetBombTimes ();
			}
		}

		private void BombReceiveRoutine (GameState state, EventArgs? e) {
			BombReceivedEventArgs? ea      = e as BombReceivedEventArgs;
			bool                   success = state.ReceiveBomb (ea!.bombtime);
			if (!success) {
				Console.WriteLine ("Outdated bomb received.");
			}
			this.GameLogic (GameAction.PASS_BOMB);
		}

		private void BombSendFailRoutine (GameState state) {
			state.FailPassBomb ();
		}

		private void PassBombRoutine (GameState state) {
			PeerInfo? peer = state.Client.GetRandomPeer();
			if (state.IsWinner () || !peer.HasValue) {
				this.GameLogic (GameAction.WIN);
				return;
			}
			if (!state.HasBomb) {
				return;
			}
			int bombTime = state.getRemainingBombTime ();
			if (bombTime <= 0) {
				this.GameLogic (GameAction.LOSE);
				return;
			}
			try {
				state.PassBomb (peer.Value, bombTime);
			} catch (Exception ex) {
				Console.WriteLine ("Could not pass bomb onwards.\n{0}\n\n{1}", ex.Message, ex.StackTrace);
				state.FailPassBomb ();
			}
		}

	}
}
