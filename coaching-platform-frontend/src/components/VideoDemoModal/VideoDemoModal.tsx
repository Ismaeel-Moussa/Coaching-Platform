import React from 'react';
import { Modal } from 'antd';
import { useTranslation } from 'react-i18next';
import './VideoDemoModal.scss';

interface VideoDemoModalProps {
  videoId: string | null;
  exerciseName: string;
  open: boolean;
  onClose: () => void;
}

const VideoDemoModal: React.FC<VideoDemoModalProps> = ({
  videoId,
  exerciseName,
  open,
  onClose,
}) => {
  const { t } = useTranslation(['common', 'athlete']);
  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={null}
      title={
        <span className="video-demo-modal__title">
          <span className="material-symbols-outlined">play_circle</span>
          {exerciseName}
        </span>
      }
      width={720}
      centered
      className="video-demo-modal"
      destroyOnHidden
    >
      {videoId ? (
        <div className="video-demo-modal__embed">
          <iframe
            src={`https://www.youtube.com/embed/${videoId}?autoplay=1&rel=0&modestbranding=1`}
            title={`${exerciseName} demo`}
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        </div>
      ) : (
        <div className="video-demo-modal__no-video">
          <span className="material-symbols-outlined">videocam_off</span>
          <p>{t('athlete:components.exerciseCard.noVideo', { defaultValue: 'No demo video available for this exercise.' })}</p>
        </div>
      )}
    </Modal>
  );
};

export default VideoDemoModal;
