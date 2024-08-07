'use client';

import React, { useState } from 'react';
import FileUpload from './components/fileUpload';
import Calculations from './components/calculations';
import IntervalUpdate from './components/intervalUpdate';
import PortfolioUpdates from './components/portfolioUpdates';
import { setUpdateInterval } from './services/api';

const HomePage = () => {
    const [updateTrigger, setUpdateTrigger] = useState(false);
    const [interval, setInterval] = useState(5); // Default interval in minutes

    const handleUploadSuccess = () => {
        setUpdateTrigger(prev => !prev);
    };

    const handleCalculate = () => {
        setUpdateTrigger(prev => !prev);
    };

    const handleIntervalChange = async (newInterval) => {
        setInterval(newInterval);
        try {
            await setUpdateInterval(newInterval);
        } catch (error) {
            console.error('Error setting update interval:', error);
        }
    };

    return (
        <div className="container mt-4">
            <div className="row">
                <div className="col-md-6 mb-4">
                    <FileUpload onUploadSuccess={handleUploadSuccess} />
                </div>
                <div className="col-md-6 mb-4">
                    <Calculations onCalculate={handleCalculate} />
                </div>
            </div>
            <div className="row">
                <div className="col-md-6">
                    <IntervalUpdate onIntervalChange={handleIntervalChange} />
                </div>
                <div className="col-md-6">
                    <PortfolioUpdates />
                </div>
            </div>
        </div>
    );
};

export default HomePage;
