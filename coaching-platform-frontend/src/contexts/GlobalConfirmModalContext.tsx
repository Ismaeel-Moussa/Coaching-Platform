import React, { createContext, useContext, useState, useCallback } from 'react';
import { Modal, Button } from 'antd';

interface ConfirmModalConfig {
  title: string;
  content: string;
  okText?: string;
  cancelText?: string;
  danger?: boolean;
  onConfirm: () => void;
}

interface GlobalConfirmModalContextValue {
  showModal: (config: ConfirmModalConfig) => void;
  hideModal: () => void;
}

const GlobalConfirmModalContext = createContext<GlobalConfirmModalContextValue | null>(null);

export const useGlobalConfirmModal = () => {
  const ctx = useContext(GlobalConfirmModalContext);
  if (!ctx) throw new Error('useGlobalConfirmModal must be used within GlobalConfirmModalProvider');
  return ctx;
};

export const GlobalConfirmModalProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [open, setOpen] = useState(false);
  const [config, setConfig] = useState<ConfirmModalConfig | null>(null);

  const showModal = useCallback((cfg: ConfirmModalConfig) => {
    setConfig(cfg);
    setOpen(true);
  }, []);

  const hideModal = useCallback(() => {
    setOpen(false);
    setConfig(null);
  }, []);

  return (
    <GlobalConfirmModalContext.Provider value={{ showModal, hideModal }}>
      {children}
      {config && (
        <Modal
          open={open}
          title={config.title}
          onCancel={hideModal}
          footer={[
            <Button key="cancel" onClick={hideModal}>
              {config.cancelText ?? 'Cancel'}
            </Button>,
            <Button
              key="ok"
              type="primary"
              danger={config.danger}
              onClick={() => {
                config.onConfirm();
                hideModal();
              }}
            >
              {config.okText ?? 'Confirm'}
            </Button>,
          ]}
        >
          <p style={{ color: 'var(--color-text-secondary)', fontSize: 14 }}>{config.content}</p>
        </Modal>
      )}
    </GlobalConfirmModalContext.Provider>
  );
};

export default GlobalConfirmModalContext;
