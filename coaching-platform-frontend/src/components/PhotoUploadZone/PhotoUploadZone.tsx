import React, { useRef, useState } from 'react';
import { Button, Progress, Spin, message as antMessage } from 'antd';
import { useTranslation } from 'react-i18next';
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
  const { t } = useTranslation(['common']);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragActive, setIsDragActive] = useState<boolean>(false);
  const [isPreparing, setIsPreparing] = useState<boolean>(false);

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

  const handleFileChange = async (selectedFile: File) => {
    // Validate size (max 10MB)
    const isLessThan10MB = selectedFile.size / 1024 / 1024 < 10;
    if (!isLessThan10MB) {
      antMessage.error(t('common:alerts.imageTooLarge'));
      return;
    }

    // Validate type
    const isJpgOrPng = selectedFile.type === 'image/jpeg' || selectedFile.type === 'image/png';
    if (!isJpgOrPng) {
      antMessage.error(t('common:alerts.imageTypeInvalid'));
      return;
    }

    setIsPreparing(true);
    try {
      // Keep an in-memory copy. Files selected from cloud-backed/mobile providers can
      // lose their original OS handle while the athlete completes the remaining steps.
      const bytes = await selectedFile.arrayBuffer();
      const durableFile = new File([bytes], selectedFile.name, {
        type: selectedFile.type,
        lastModified: selectedFile.lastModified,
      });
      onFileSelect(durableFile);
    } catch {
      antMessage.error(t('common:alerts.imageReadFailed'));
    } finally {
      setIsPreparing(false);
    }
  };

  const onInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.currentTarget.files?.[0];
    e.currentTarget.value = '';
    if (selectedFile) void handleFileChange(selectedFile);
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
      void handleFileChange(e.dataTransfer.files[0]);
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
        disabled={uploading || isPreparing}
      />

      {uploading || isPreparing ? (
        <div className="upload-zone__progress-container">
          {uploadProgress > 0 ? (
            <Progress
              type="circle"
              percent={uploadProgress}
              strokeColor="var(--color-gold)"
              size={80}
            />
          ) : (
            <Spin size="large" />
          )}
          <span className="upload-zone__upload-label">
            {t('common:photoUpload.uploadingAngle', { angle: t(`common:photoUpload.angles.${angle.toLowerCase()}`) })}
          </span>
        </div>
      ) : displayUrl ? (
        <div className="upload-zone__preview">
          <img src={displayUrl} alt={`${angle} progress preview`} className="upload-zone__preview-img" />
          <div className="upload-zone__overlay">
            <span className="upload-zone__angle-badge">
              {t('common:photoUpload.angleView', { angle: t(`common:photoUpload.angles.${angle.toLowerCase()}`) })}
            </span>
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
          <span className="upload-zone__empty-label">
            {t('common:photoUpload.angleView', { angle: t(`common:photoUpload.angles.${angle.toLowerCase()}`) })}
          </span>
          <p className="upload-zone__empty-hint">{t('common:photoUpload.dragDropOrClick')}</p>
          <span className="upload-zone__empty-limit">{t('common:photoUpload.limits')}</span>
        </div>
      )}
    </div>
  );
};

export default PhotoUploadZone;
