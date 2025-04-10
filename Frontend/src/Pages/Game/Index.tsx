import React, { useState, useEffect } from 'react';
import { Level } from '../../Types/Level';
import api from '../../services/api';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './Styles.css';

interface GameItem {
  id: string;
  description: string;
  level: Level;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
}

interface TaskCompletion {
  [key: string]: {
    monday: boolean;
    tuesday: boolean;
    wednesday: boolean;
    thursday: boolean;
    friday: boolean;
  };
}

type DayOfWeek = 'monday' | 'tuesday' | 'wednesday' | 'thursday' | 'friday';

const STORAGE_KEY = 'gameActivities';
const COMPLETION_KEY = 'gameCompletions';

const initialGames: GameItem[] = [
  {
    id: '1',
    description: 'Match pairs of cards',
    level: Level.Easy,
    monday: false,
    tuesday: false,
    wednesday: false,
    thursday: false,
    friday: false
  },
  {
    id: '2',
    description: 'Find hidden words',
    level: Level.Medium,
    monday: false,
    tuesday: false,
    wednesday: false,
    thursday: false,
    friday: false
  },
  {
    id: '3',
    description: 'Solve math problems',
    level: Level.Hard,
    monday: false,
    tuesday: false,
    wednesday: false,
    thursday: false,
    friday: false
  }
];

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
    return <span className="done-text">DONE!</span>;
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
  const [games, setGames] = useState<GameItem[]>(() => {
    const savedGames = localStorage.getItem(STORAGE_KEY);
    return savedGames ? JSON.parse(savedGames) : initialGames;
  });

  const [completions, setCompletions] = useState<TaskCompletion>(() => {
    const savedCompletions = localStorage.getItem(COMPLETION_KEY);
    return savedCompletions ? JSON.parse(savedCompletions) : {};
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(games));
  }, [games]);

  useEffect(() => {
    localStorage.setItem(COMPLETION_KEY, JSON.stringify(completions));
  }, [completions]);

  const handleDayToggle = (index: number, day: DayOfWeek) => {
    const game = games[index];
    const gameCompletions = completions[game.id] || {
      monday: false,
      tuesday: false,
      wednesday: false,
      thursday: false,
      friday: false
    };

    setCompletions(prev => ({
      ...prev,
      [game.id]: {
        ...gameCompletions,
        [day]: !gameCompletions[day]
      }
    }));
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
        const response = await api.get('Game');
        setGames(response.data);
        localStorage.removeItem(COMPLETION_KEY);
        setCompletions({});
        toast.success('Games reset successfully!');
      } catch (err) {
        const errorMessage = 'Error loading games from API. Using local data.';
        setError(errorMessage);
        console.error('Error loading games:', err);
        const resetGames = games.map(game => ({
          ...game,
          monday: false,
          tuesday: false,
          wednesday: false,
          thursday: false,
          friday: false
        }));
        setGames(resetGames);
        localStorage.removeItem(COMPLETION_KEY);
        setCompletions({});
        toast.error(errorMessage);
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

  const isTaskCompleted = (gameId: string, day: DayOfWeek): boolean => {
    const gameCompletions = completions[gameId];
    return gameCompletions ? gameCompletions[day] : false;
  };

  return (
    <div className="game-container">
      <ToastContainer />
      <h1>Games Management</h1>
      <div className="game-content">
        <div className="button-container">
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
                {isColumnComplete('monday') && <span className="done-text-footer">DONE!</span>}
              </td>
              <td>
                {isColumnComplete('tuesday') && <span className="done-text-footer">DONE!</span>}
              </td>
              <td>
                {isColumnComplete('wednesday') && <span className="done-text-footer">DONE!</span>}
              </td>
              <td>
                {isColumnComplete('thursday') && <span className="done-text-footer">DONE!</span>}
              </td>
              <td>
                {isColumnComplete('friday') && <span className="done-text-footer">DONE!</span>}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Game; 