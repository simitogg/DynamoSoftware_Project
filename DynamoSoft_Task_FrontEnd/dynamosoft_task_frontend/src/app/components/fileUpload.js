'use client';

import React, { useState } from 'react';
import { uploadFile } from '../services/api';
import PropTypes from 'prop-types';

const FileUpload = ({ onUploadSuccess }) => {
    const [file, setFile] = useState(null);

    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    };

    const handleUpload = async () => {
        if (!file) {
            alert('Please select a file to upload.');
            return;
        }
        try {
            const response = await uploadFile(file);
            alert('File uploaded successfully.');
            if (onUploadSuccess) {
                onUploadSuccess();
            }
        } catch (error) {
            console.error('Error uploading file:', error);
            alert('Failed to upload file.' + error);
        }
    };

    return (
        <div className="container mt-3">
            <div className="form-group">
                <label htmlFor="fileUpload">Upload Portfolio</label>
                <input
                    type="file"
                    className="form-control-file"
                    id="fileUpload"
                    onChange={handleFileChange}
                />
            </div>
            <button className="btn btn-primary" onClick={handleUpload}>Upload</button>
        </div>
    );
};

FileUpload.propTypes = {
    onUploadSuccess: PropTypes.func.isRequired,
};

export default FileUpload;
