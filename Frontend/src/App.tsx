import React from 'react';
import { HashRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Home from './Pages/Home/Index';
import Activity from './Pages/Activity/Index';
import Tests from './Pages/Tests/Index';
import Game from './Pages/Game/Index';
import './App.css';

const App: React.FC = () => {
  return (
    <Router>
      <div className="app">
        <nav className="nav-menu">
          <ul>
            <li><Link to="/">Home</Link></li>
            <li><Link to="/game">Game</Link></li>
            <li><Link to="/activity">Activity</Link></li>
            <li><Link to="/tests">Tests</Link></li>
          </ul>
        </nav>
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/game" element={<Game />} />
            <Route path="/activity" element={<Activity />} />
            <Route path="/tests" element={<Tests />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
};

export default App;
