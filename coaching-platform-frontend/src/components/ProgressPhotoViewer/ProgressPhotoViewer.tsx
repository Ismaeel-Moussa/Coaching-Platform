import React, { useState, useEffect } from 'react';
import { Image } from 'antd';
import { useTranslation } from 'react-i18next';
import './ProgressPhotoViewer.scss';

export interface ProgressPhotoItem {
  angle: 'Front' | 'Side' | 'Back' | string;
  url?: string | null;
  file?: File | null;
}

interface ProgressPhotoViewerProps {
  photos: ProgressPhotoItem[];
  variant?: 'grid' | 'thumb'; // 'grid' for full size display, 'thumb' for CheckInCard mini-display
}

const PhotoItem: React.FC<{ item: ProgressPhotoItem; variant: 'grid' | 'thumb' }> = ({ item, variant }) => {
  const { t } = useTranslation(['athlete', 'common']);
  const [localUrl, setLocalUrl] = useState<string | null>(null);

  useEffect(() => {
    if (item.file) {
      const objectUrl = URL.createObjectURL(item.file);
      setLocalUrl(objectUrl);
      return () => URL.revokeObjectURL(objectUrl);
    }
    setLocalUrl(null);
  }, [item.file]);

  const displayUrl = localUrl || item.url;
  const angleLabel = item.angle === 'Front' 
    ? t('athlete:checkIn.frontView', 'Front') 
    : item.angle === 'Side' 
    ? t('athlete:checkIn.sideView', 'Side') 
    : item.angle === 'Back'
    ? t('athlete:checkIn.backView', 'Back')
    : item.angle;

  if (variant === 'thumb') {
    if (!displayUrl) return null;
    return (
      <div className="progress-photo-viewer__thumb-item">
        <Image src={displayUrl} alt={`${item.angle} view`} />
        <span className="progress-photo-viewer__thumb-label">{item.angle}</span>
      </div>
    );
  }

  return (
    <div className="progress-photo-viewer__grid-item">
      <span className="progress-photo-viewer__grid-label">{angleLabel}</span>
      {displayUrl ? (
        <Image src={displayUrl} alt={`${item.angle} View`} className="progress-photo-viewer__grid-img" />
      ) : (
        <div className="progress-photo-viewer__grid-empty">
          {t('athlete:checkIn.noPhotoSelected', 'No photo')}
        </div>
      )}
    </div>
  );
};

export const ProgressPhotoViewer: React.FC<ProgressPhotoViewerProps> = ({ photos, variant = 'grid' }) => {
  const { t } = useTranslation(['coach']);
  
  const hasPhotos = photos.some(p => p.url || p.file);

  if (variant === 'thumb' && !hasPhotos) {
    return (
      <div className="progress-photo-viewer__no-photos">
        <span className="material-symbols-outlined">hide_image</span>
        <span>{t('coach:clientDetail.noPhotos', 'No Photos')}</span>
      </div>
    );
  }

  return (
    <Image.PreviewGroup>
      <div className={`progress-photo-viewer progress-photo-viewer--${variant}`}>
        {photos.map((photo, index) => (
          <PhotoItem key={photo.angle + index} item={photo} variant={variant} />
        ))}
      </div>
    </Image.PreviewGroup>
  );
};

export default ProgressPhotoViewer;
