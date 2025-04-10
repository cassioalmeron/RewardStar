import React, { useState, useEffect } from 'react';
import api from '../../services/api';
import { Level } from '../../Types/Level';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './Styles.css';

interface ActivityItem {
  id: number;
  description: string;
  level: Level;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
  active: boolean;
  position: number;
}

const Activity: React.FC = () => {
  const [activities, setActivities] = useState<ActivityItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const fetchActivities = async () => {
      try {
        const response = await api.get('/Activities');
        const activitiesWithPosition = response.data.map((activity: ActivityItem, index: number) => ({
          ...activity,
          position: index + 1
        }));
        setActivities(activitiesWithPosition);
      } catch (err) {
        setError('Error loading activities. Please try again.');
        console.error('Error loading activities:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchActivities();
  }, []);

  const handleInputChange = (index: number, field: keyof ActivityItem, value: string | boolean | Level) => {
    const newActivities = [...activities];
    newActivities[index] = {
      ...newActivities[index],
      [field]: value
    };
    setActivities(newActivities);
  };

  const handleAddActivity = () => {
    const newActivity: ActivityItem = {
      id: Date.now() * -1,
      description: "New Activity",
      level: Level.Easy,
      monday: true,
      tuesday: true,
      wednesday: true,
      thursday: true,
      friday: true,
      active: true,
      position: activities.length + 1
    };
    setActivities([...activities, newActivity]);
  };

  const handleDeleteActivity = (index: number) => {
    toast.info(
      <div>
        <p>Are you sure you want to delete this activity?</p>
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px', marginTop: '8px' }}>
          <button 
            onClick={() => {
              const newActivities = activities.filter((_, i) => i !== index);
              const updatedActivities = newActivities.map((activity, i) => ({
                ...activity,
                position: i + 1
              }));
              setActivities(updatedActivities);
              toast.dismiss();
              toast.success('Activity deleted successfully!');
            }}
            style={{ padding: '4px 8px', background: '#dc3545', color: 'white' }}
          >
            Delete
          </button>
          <button 
            onClick={() => toast.dismiss()}
            style={{ padding: '4px 8px', background: '#ccc' }}
          >
            Cancel
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
  };

  const handleSaveActivities = async () => {
    setSaving(true);
    setError(null);

    try {
      const sortedActivities = [...activities].sort((a, b) => a.position - b.position);
      const activitiesToSave = sortedActivities.map(activity => ({
        ...activity,
        id: activity.id < 0 ? 0 : activity.id
      }));
      console.log(activitiesToSave);
      await api.post('/Activities', activitiesToSave);
      setActivities(activitiesToSave);
      toast.success('Activities saved successfully!');
    } catch (err) {
      const errorMessage = 'Error saving activities. Please try again.';
      console.error('Error saving activities:', err);
      toast.error(errorMessage);
    } finally {
      setSaving(false);
    }
  };

  const getLevelName = (level: Level): string => {
    switch (level) {
      case Level.Easy:
        return 'Easy';
      case Level.Medium:
        return 'Medium';
      case Level.Hard:
        return 'Hard';
      default:
        return 'Unknown';
    }
  };

  if (loading) {
    return (
      <div className="activity-container">
        <h1>Activity Schedule</h1>
        <div className="loading">Loading activities...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="activity-container">
        <h1>Activity Schedule</h1>
        <div className="error">{error}</div>
      </div>
    );
  }

  return (
    <div className="activity-container">
      <ToastContainer />
      <h1>Activity Schedule</h1>
      <div className="activity-content">
        <div className="button-container">
          <button onClick={handleAddActivity} className="add-activity-btn">
            Add new Activity
          </button>
          <button 
            onClick={handleSaveActivities} 
            className="save-activity-btn"
            disabled={saving}
          >
            {saving ? 'Saving...' : 'Save'}
          </button>
        </div>
        <table className="activity-table">
          <thead>
            <tr>
              <th>Position</th>
              <th>Description</th>
              <th>Level</th>
              <th>Monday</th>
              <th>Tuesday</th>
              <th>Wednesday</th>
              <th>Thursday</th>
              <th>Friday</th>
              <th>Active</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {[...activities]
              .sort((a, b) => a.position - b.position)
              .map((activity, index) => (
              <tr key={activity.id} className="activity-row">
                <td>
                  <input
                    type="number"
                    value={activity.position}
                    onChange={(e) => handleInputChange(index, 'position', parseInt(e.target.value) || 0)}
                    className="activity-input position-input"
                    min="1"
                  />
                </td>
                <td>
                  <input
                    type="text"
                    value={activity.description}
                    onChange={(e) => handleInputChange(index, 'description', e.target.value)}
                    className="activity-input"
                  />
                </td>
                <td>
                  <select
                    value={activity.level}
                    onChange={(e) => handleInputChange(index, 'level', Number(e.target.value) as Level)}
                    className={`activity-select level-${activity.level}`}
                  >
                    <option value={Level.Easy}>{getLevelName(Level.Easy)}</option>
                    <option value={Level.Medium}>{getLevelName(Level.Medium)}</option>
                    <option value={Level.Hard}>{getLevelName(Level.Hard)}</option>
                  </select>
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.monday}
                    onChange={(e) => handleInputChange(index, 'monday', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.tuesday}
                    onChange={(e) => handleInputChange(index, 'tuesday', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.wednesday}
                    onChange={(e) => handleInputChange(index, 'wednesday', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.thursday}
                    onChange={(e) => handleInputChange(index, 'thursday', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.friday}
                    onChange={(e) => handleInputChange(index, 'friday', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <input
                    type="checkbox"
                    checked={activity.active}
                    onChange={(e) => handleInputChange(index, 'active', e.target.checked)}
                    className="activity-checkbox"
                  />
                </td>
                <td>
                  <button 
                    onClick={() => handleDeleteActivity(index)}
                    className="delete-activity-btn"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Activity; 