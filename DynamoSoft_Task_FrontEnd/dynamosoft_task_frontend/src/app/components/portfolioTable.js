'use client';

import React from 'react';
import PropTypes from 'prop-types';

const PortfolioTable = ({ details }) => {
    if (!details || !Array.isArray(details) || details.length === 0) {
        return <div>No portfolio data available.</div>;
    }

    return (
        <table className="table table-striped">
            <thead>
                <tr>
                    <th>Symbol</th>
                    <th>Initial Buy Price</th>
                    <th>Quantity</th>
                    <th>Current Price</th>
                    <th>Current Value</th>
                    <th>Change (%)</th>
                </tr>
            </thead>
            <tbody>
                {details.map((item, index) => (
                    <tr key={index}>
                        <td>{item.symbol}</td>
                        <td>${item.initialBuyPrice.toFixed(2)}</td>
                        <td>{item.quantity.toFixed(2)}</td>
                        <td>${item.currentPrice.toFixed(2)}</td>
                        <td>${item.currentValue.toFixed(2)}</td>
                        <td>{item.change.toFixed(2)}%</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
};

PortfolioTable.propTypes = {
    details: PropTypes.arrayOf(
        PropTypes.shape({
            symbol: PropTypes.string.isRequired,
            initialBuyPrice: PropTypes.number.isRequired,
            quantity: PropTypes.number.isRequired,
            currentPrice: PropTypes.number.isRequired,
            currentValue: PropTypes.number.isRequired,
            change: PropTypes.number.isRequired
        })
    ).isRequired
};

export default PortfolioTable;
