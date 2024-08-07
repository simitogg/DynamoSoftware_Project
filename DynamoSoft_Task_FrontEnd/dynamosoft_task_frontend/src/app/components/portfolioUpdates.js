'use client';
import React, { useState, useCallback } from 'react';
import useSignalR from '../hooks/useSignalR';
import PortfolioTable from './portfolioTable';

const PortfolioUpdates = () => {
    const [portfolioData, setPortfolioData] = useState([]);

    // Use useCallback to memoize the event handlers
    const eventHandlers = useCallback({
        ReceivePortfolioUpdate: (updatedPortfolio) => {
            setPortfolioData(updatedPortfolio);
        }
    }, []);

    useSignalR('http://localhost:5013/portfolioHub', eventHandlers);

    return (
        <div>
            <h1>Portfolio Updates</h1>
            <PortfolioTable details={portfolioData} />
        </div>
    );
};

export default PortfolioUpdates;
