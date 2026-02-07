import React, { useState, useEffect, useCallback } from 'react';
import { Level } from '../../Types/Level';
import api from '../../services/api';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { StarIcon } from '../../Components/icons';
import './styles.css';

interface GameItem {
  id: number;
  description: string;
  level: Level;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
}

interface CompletionDays {
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
}

interface TaskCompletion {
  [key: number]: CompletionDays;
}

type DayOfWeek = 'monday' | 'tuesday' | 'wednesday' | 'thursday' | 'friday';

enum RewardType {
  MMs = 1,
  Bibs = 2,
  Caramel = 3
}

const REWARD_COSTS: Record<RewardType, number> = {
  [RewardType.MMs]: 2,
  [RewardType.Bibs]: 4,
  [RewardType.Caramel]: 6
};

interface GameCellProps {
  game: GameItem;
  day: DayOfWeek;
  index: number;
  onToggle: (index: number, day: DayOfWeek) => void;
  isCompleted: boolean;
}

const GameCell: React.FC<GameCellProps> = ({ game, day, index, onToggle, isCompleted }) => {
  if (game[day] === false) {
    return <span className="disabled-text">X</span>;
  }

  if (isCompleted) {
    // Determine star color based on game level
    let starColor = '#facc15'; // default yellow
    switch (game.level) {
      case Level.Easy:
        starColor = '#66bb6a'; // green
        break;
      case Level.Medium:
        starColor = '#42a5f5'; // blue
        break;
      case Level.Hard:
        starColor = '#ffa726'; // orange
        break;
    }
    return <StarIcon color={starColor} size={32} />;
  }

  return (
    <button
      onClick={() => onToggle(index, day)}
      className="game-button"
    >
      Click
    </button>
  );
};

const Game: React.FC = () => {
  const [games, setGames] = useState<GameItem[]>([]);
  const [completions, setCompletions] = useState<TaskCompletion>({});
  const [totalPoints, setTotalPoints] = useState<number>(0);
  const [balance, setBalance] = useState<number>(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadGameData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [activitiesRes, stateRes] = await Promise.all([
        api.get('Game'),
        api.get('Game/state')
      ]);

      setGames(activitiesRes.data);

      const state = stateRes.data;
      setCompletions(state.completions || {});
      setTotalPoints(state.totalPoints || 0);
      setBalance(state.balance || 0);
    } catch (err) {
      setError('Failed to load game data. Please try again.');
      toast.error('Failed to load game data.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadGameData();
  }, [loadGameData]);

  const handleDayToggle = async (index: number, day: DayOfWeek) => {
    const game = games[index];
    const gameCompletions = completions[game.id] || {
      monday: false,
      tuesday: false,
      wednesday: false,
      thursday: false,
      friday: false
    };

    const isCurrentlyCompleted = gameCompletions[day];

    // Optimistic UI update
    const prevCompletions = { ...completions };
    const prevTotalPoints = totalPoints;
    const prevBalance = balance;

    setCompletions(prev => ({
      ...prev,
      [game.id]: {
        ...gameCompletions,
        [day]: !isCurrentlyCompleted
      }
    }));

    // Optimistic points calculation (matches backend POINTS_PER_LEVEL)
    const pointsMap: Record<Level, number> = {
      [Level.Easy]: 1,
      [Level.Medium]: 2,
      [Level.Hard]: 3
    };
    const points = pointsMap[game.level] || 0;

    if (isCurrentlyCompleted) {
      setTotalPoints(prev => prev - points);
      setBalance(prev => prev - points);
    } else {
      setTotalPoints(prev => prev + points);
      setBalance(prev => prev + points);
    }

    try {
      const response = await api.post('Game/toggle', {
        activityId: game.id,
        day: day
      });

      // Sync with server response
      setTotalPoints(response.data.totalPoints);
      setBalance(response.data.balance);
    } catch (err) {
      // Revert optimistic update
      setCompletions(prevCompletions);
      setTotalPoints(prevTotalPoints);
      setBalance(prevBalance);
      toast.error('Failed to save completion. Please try again.');
    }
  };

  const handleReset = async () => {
    const confirmReset = () => new Promise((resolve) => {
      toast.info(
        <div>
          <p>Are you sure you want to reset all games?</p>
          <p>This action cannot be undone.</p>
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px', marginTop: '8px' }}>
            <button
              onClick={() => {
                toast.dismiss();
                resolve(false);
              }}
              style={{ padding: '4px 8px', background: '#ccc' }}
            >
              Cancel
            </button>
            <button
              onClick={() => {
                toast.dismiss();
                resolve(true);
              }}
              style={{ padding: '4px 8px', background: '#dc3545', color: 'white' }}
            >
              Reset
            </button>
          </div>
        </div>,
        {
          position: "top-center",
          autoClose: false,
          closeOnClick: false,
          draggable: false,
          closeButton: false
        }
      );
    });

    const confirmed = await confirmReset();
    if (confirmed) {
      setLoading(true);
      setError(null);
      try {
        await api.post('Game/reset');
        await loadGameData();
        toast.success('Games reset successfully!');
      } catch (err) {
        setError('Error resetting game. Please try again.');
        toast.error('Error resetting game. Please try again.');
      } finally {
        setLoading(false);
      }
    }
  };

  const getDifficultyClass = (level: Level): string => {
    switch (level) {
      case Level.Easy:
        return 'difficulty-easy';
      case Level.Medium:
        return 'difficulty-medium';
      case Level.Hard:
        return 'difficulty-hard';
      default:
        return '';
    }
  };

  const isColumnComplete = (day: DayOfWeek): boolean => {
    return games.every(game => {
      if (game[day] === false) {
        return true;
      }
      const gameCompletions = completions[game.id];
      return gameCompletions && gameCompletions[day];
    });
  };

  const isTaskCompleted = (gameId: number, day: DayOfWeek): boolean => {
    const gameCompletions = completions[gameId];
    return gameCompletions ? gameCompletions[day] : false;
  };

  const handleClaimReward = async (rewardType: RewardType, rewardName: string, cost: number) => {
    if (balance < cost) {
      toast.error(`Insufficient balance. You need ${cost} points but have ${balance}.`);
      return;
    }

    // Optimistic UI update
    const prevBalance = balance;
    setBalance(prev => prev - cost);

    try {
      await api.post('Game/claim', {
        rewardType: rewardType
      });
      toast.success(`Successfully claimed ${rewardName}! (-${cost} points)`);
    } catch (err) {
      // Revert optimistic update
      setBalance(prevBalance);
      toast.error(`Failed to claim ${rewardName}. Please try again.`);
    }
  };

  if (loading && games.length === 0) {
    return (
      <div className="game-container">
        <h1>Week Game</h1>
        <div className="game-content">
          <p>Loading game data...</p>
        </div>
      </div>
    );
  }

  if (!loading && games.length === 0) {
    return (
      <div className="game-container">
        <h1>Week Game</h1>
        <div className="game-content">
          <p>No activities configured yet. Please set up your activities in the Activity page first.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="game-container">
      <ToastContainer />
      <h1>Week Game</h1>
      <div className="game-content">
        <div className="button-container">
          <div className="points-display">
            Total Points: <span className="points-value">{totalPoints}</span>
          </div>
          <button
            onClick={handleReset}
            className="reset-button"
            disabled={loading}
          >
            {loading ? 'Loading...' : 'Reset Game'}
          </button>
        </div>
        {error && (
          <div className="error">{error}</div>
        )}
        <table className="game-table">
          <thead>
            <tr>
              <th>Description</th>
              <th>Monday</th>
              <th>Tuesday</th>
              <th>Wednesday</th>
              <th>Thursday</th>
              <th>Friday</th>
            </tr>
          </thead>
          <tbody>
            {games.map((game, index) => (
              <tr key={game.id} className="game-row">
                <td className={`game-select ${getDifficultyClass(game.level)}`}>
                  {game.description}
                </td>
                <td>
                  <GameCell
                    game={game}
                    day="monday"
                    index={index}
                    onToggle={handleDayToggle}
                    isCompleted={isTaskCompleted(game.id, 'monday')}
                  />
                </td>
                <td>
                  <GameCell
                    game={game}
                    day="tuesday"
                    index={index}
                    onToggle={handleDayToggle}
                    isCompleted={isTaskCompleted(game.id, 'tuesday')}
                  />
                </td>
                <td>
                  <GameCell
                    game={game}
                    day="wednesday"
                    index={index}
                    onToggle={handleDayToggle}
                    isCompleted={isTaskCompleted(game.id, 'wednesday')}
                  />
                </td>
                <td>
                  <GameCell
                    game={game}
                    day="thursday"
                    index={index}
                    onToggle={handleDayToggle}
                    isCompleted={isTaskCompleted(game.id, 'thursday')}
                  />
                </td>
                <td>
                  <GameCell
                    game={game}
                    day="friday"
                    index={index}
                    onToggle={handleDayToggle}
                    isCompleted={isTaskCompleted(game.id, 'friday')}
                  />
                </td>
              </tr>
            ))}
            <tr className="footer-row">
              <td></td>
              <td>
                {isColumnComplete('monday') && <StarIcon color="#facc15" size={32} />}
              </td>
              <td>
                {isColumnComplete('tuesday') && <StarIcon color="#facc15" size={32} />}
              </td>
              <td>
                {isColumnComplete('wednesday') && <StarIcon color="#facc15" size={32} />}
              </td>
              <td>
                {isColumnComplete('thursday') && <StarIcon color="#facc15" size={32} />}
              </td>
              <td>
                {isColumnComplete('friday') && <StarIcon color="#facc15" size={32} />}
              </td>
            </tr>
          </tbody>
        </table>

        {/* Rewards Section */}
        <div className="rewards-section">
          <h2 className="rewards-title">Rewards</h2>

          {/* Balance Display */}
          <div className="balance-display">
            <div className="balance-text">
              Available Balance: <span className="balance-value">{balance}</span> points
            </div>
          </div>

          {/* Reward Claim Buttons */}
          <div className="reward-buttons">
            <button
              onClick={() => handleClaimReward(RewardType.MMs, 'M&Ms', REWARD_COSTS[RewardType.MMs])}
              className="reward-button reward-button-mms"
              disabled={balance < REWARD_COSTS[RewardType.MMs]}
            >
              Claim M&Ms<br />
              <span className="reward-cost">(2 points)</span>
            </button>
            <button
              onClick={() => handleClaimReward(RewardType.Bibs, 'Bibs', REWARD_COSTS[RewardType.Bibs])}
              className="reward-button reward-button-bibs"
              disabled={balance < REWARD_COSTS[RewardType.Bibs]}
            >
              Claim Bibs<br />
              <span className="reward-cost">(4 points)</span>
            </button>
            <button
              onClick={() => handleClaimReward(RewardType.Caramel, 'Caramel', REWARD_COSTS[RewardType.Caramel])}
              className="reward-button reward-button-caramel"
              disabled={balance < REWARD_COSTS[RewardType.Caramel]}
            >
              Claim Caramel<br />
              <span className="reward-cost">(6 points)</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Game;
