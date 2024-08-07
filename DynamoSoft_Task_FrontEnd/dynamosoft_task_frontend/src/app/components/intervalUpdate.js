'use client';

import React, { useState } from 'react';
import PropTypes from 'prop-types';

const IntervalUpdate = ({ onIntervalChange }) => {
    const [interval, setInterval] = useState(5); // Default interval in minutes

    const handleChange = (e) => {
        setInterval(parseInt(e.target.value, 10));
    };

    const handleApplyInterval = async () => {
        try {
            // Call the onIntervalChange function, which should handle the API request
            await onIntervalChange(interval);
            alert(`Update interval set to ${interval} minutes.`);
        } catch (error) {
            console.error('Error updating interval:', error);
            alert('Failed to update interval.');
        }
    };

    return (
        <div className="form-group">
            <label htmlFor="interval">Update Interval (minutes):</label>
            <div className="input-group">
                <input
                    type="number"
                    className="form-control"
                    id="interval"
                    value={interval}
                    onChange={handleChange}
                    min="1" // Set a minimum value for the interval
                />
                <div className="input-group-append">
                    <button
                        className="btn btn-primary"
                        onClick={handleApplyInterval}
                    >
                        Apply Interval
                    </button>
                </div>
            </div>
        </div>
    );
};

IntervalUpdate.propTypes = {
    onIntervalChange: PropTypes.func.isRequired,
};

export default IntervalUpdate;
