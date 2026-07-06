import React, { useRef, useState } from 'react';
import { Button, Progress, message as antMessage } from 'antd';
import './PhotoUploadZone.scss';

interface PhotoUploadZoneProps {
  angle: 'Front' | 'Side' | 'Back';
  file: File | null;
  onFileSelect: (file: File) => void;
  onDelete: () => void;
  existingUrl?: string | null;
  uploading?: boolean;
  uploadProgress?: number;
}

const PhotoUploadZone: React.FC<PhotoUploadZoneProps> = ({
  angle,
  file,
  onFileSelect,
  onDelete,
  existingUrl,
  uploading = false,
  uploadProgress = 0,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragActive, setIsDragActive] = useState<boolean>(false);

  // local URL preview for newly selected file
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  React.useEffect(() => {
    if (file) {
      const url = URL.createObjectURL(file);
      setPreviewUrl(url);
      return () => URL.revokeObjectURL(url);
    } else {
      setPreviewUrl(null);
    }
  }, [file]);

  const handleFileChange = (selectedFile: File) => {
    // Validate size (max 10MB)
    const isLessThan10MB = selectedFile.size / 1024 / 1024 < 10;
    if (!isLessThan10MB) {
      antMessage.error('Image must be smaller than 10MB!');
      return;
    }

    // Validate type
    const isJpgOrPng = selectedFile.type === 'image/jpeg' || selectedFile.type === 'image/png';
    if (!isJpgOrPng) {
      antMessage.error('You can only upload JPG or PNG files!');
      return;
    }

    onFileSelect(selectedFile);
  };

  const onInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      handleFileChange(e.target.files[0]);
    }
  };

  const onDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragActive(true);
  };

  const onDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragActive(false);
  };

  const onDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragActive(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileChange(e.dataTransfer.files[0]);
    }
  };

  const triggerBrowse = () => {
    fileInputRef.current?.click();
  };

  const displayUrl = previewUrl || existingUrl;

  return (
    <div
      className={`upload-zone ${isDragActive ? 'upload-zone--active' : ''} ${
        displayUrl ? 'upload-zone--has-preview' : ''
      } ${uploading ? 'upload-zone--uploading' : ''}`}
      onDragOver={onDragOver}
      onDragLeave={onDragLeave}
      onDrop={onDrop}
    >
      <input
        type="file"
        ref={fileInputRef}
        onChange={onInputChange}
        accept="image/jpeg,image/png"
        style={{ display: 'none' }}
        disabled={uploading}
      />

      {uploading ? (
        <div className="upload-zone__progress-container">
          <Progress
            type="circle"
            percent={uploadProgress}
            strokeColor="var(--color-gold)"
            size={80}
          />
          <span className="upload-zone__upload-label">Uploading {angle} View...</span>
        </div>
      ) : displayUrl ? (
        <div className="upload-zone__preview">
          <img src={displayUrl} alt={`${angle} progress preview`} className="upload-zone__preview-img" />
          <div className="upload-zone__overlay">
            <span className="upload-zone__angle-badge">{angle} View</span>
            <div className="upload-zone__actions">
              <Button
                type="primary"
                danger
                shape="circle"
                icon={<span className="material-symbols-outlined">delete</span>}
                onClick={(e) => {
                  e.stopPropagation();
                  onDelete();
                }}
                className="upload-zone__delete-btn"
              />
            </div>
          </div>
        </div>
      ) : (
        <div className="upload-zone__empty" onClick={triggerBrowse}>
          <span className="material-symbols-outlined upload-zone__empty-icon">photo_camera</span>
          <span className="upload-zone__empty-label">{angle} View</span>
          <p className="upload-zone__empty-hint">Drag & drop or click to upload</p>
          <span className="upload-zone__empty-limit">JPG/PNG, max 10MB</span>
        </div>
      )}
    </div>
  );
};

export default PhotoUploadZone;
