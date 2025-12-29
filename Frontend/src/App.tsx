import React from 'react';
import { HashRouter as Router, Routes, Route } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import Layout from './Components/layout/Layout';
import Home from './Pages/Home';
import Activity from './Pages/Activity';
import Game from './Pages/Game';
import Settings from './Pages/Settings';
import './App.css';

const App: React.FC = () => {
  return (
    <Router>
      <div className="app">
        <Layout>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/game" element={<Game />} />
            <Route path="/activity" element={<Activity />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </Layout>
        <ToastContainer />
      </div>
    </Router>
  );
};

export default App;
