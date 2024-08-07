'use client';

import React, { useState } from 'react';
import { fetchPortfolioValue } from '../services/api';
import PortfolioTable from './portfolioTable';

const Calculations = () => {
    const [portfolioData, setPortfolioData] = useState([]);
    const [loading, setLoading] = useState(false);

    const handleCalculate = async () => {
        setLoading(true);
        try {
            const data = await fetchPortfolioValue();
            setPortfolioData(data);
        } catch (error) {
            console.error('Error fetching portfolio value:', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div>
            <button className="btn btn-primary mb-3" onClick={handleCalculate} disabled={loading}>
                {loading ? 'Calculating...' : 'Calculate Portfolio Value'}
            </button>

            {loading && <div>Loading...</div>}

            {portfolioData.length > 0 ? (
                <div>
                    <h2>Portfolio Value</h2>
                    <PortfolioTable details={portfolioData} />
                </div>
            ) : (
                !loading && <div>Press the button to calculate portfolio value.</div>
            )}
        </div>
    );
};

export default Calculations;
