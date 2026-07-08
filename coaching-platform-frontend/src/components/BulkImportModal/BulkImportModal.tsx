import React, { useState } from 'react';
import { Modal, Tabs, Upload, Input, Button, message, List, Alert } from 'antd';
import { useBulkImportFoods } from '../../hooks/useFoods/useFoods';
import type { BulkImportResultDto } from '../../types/Food';
import './BulkImportModal.scss';

interface BulkImportModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess?: () => void;
}

const BulkImportModal: React.FC<BulkImportModalProps> = ({ visible, onCancel, onSuccess }) => {
  const [activeTab, setActiveTab] = useState<string>('file');
  const [fileList, setFileList] = useState<any[]>([]);
  const [pasteContent, setPasteContent] = useState<string>('');
  const [csvPreview, setCsvPreview] = useState<string[][]>([]);
  const [importResult, setImportResult] = useState<BulkImportResultDto | null>(null);

  const importMutation = useBulkImportFoods();

  const handleTabChange = (key: string) => {
    setActiveTab(key);
    setImportResult(null);
    setCsvPreview([]);
  };

  // Parses CSV string to generate a preview grid (first 5 rows)
  const parseCSVPreview = (text: string) => {
    const lines = text.split('\n').map((line) => line.trim()).filter(Boolean);
    const grid = lines.slice(0, 6).map((line) => {
      // Basic comma-splitting supporting quotes is ideal, but simple split works for preview
      return line.split(',');
    });
    setCsvPreview(grid);
  };

  const beforeUpload = (file: File) => {
    const isCSV = file.type === 'text/csv' || file.name.endsWith('.csv');
    if (!isCSV) {
      message.error('You can only upload CSV files!');
      return Upload.LIST_IGNORE;
    }
    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      message.error('File must be smaller than 10MB!');
      return Upload.LIST_IGNORE;
    }

    setFileList([file]);

    // Read file client-side for preview
    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      parseCSVPreview(text);
    };
    reader.readAsText(file);

    return false; // prevent automatic upload
  };

  const handlePasteChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const val = e.target.value;
    setPasteContent(val);
    if (val.trim()) {
      parseCSVPreview(val);
    } else {
      setCsvPreview([]);
    }
  };

  const handleImport = async () => {
    let fileToUpload: File | null = null;

    if (activeTab === 'file') {
      if (fileList.length === 0) {
        message.error('Please select a CSV file first.');
        return;
      }
      fileToUpload = fileList[0];
    } else {
      if (!pasteContent.trim()) {
        message.error('Please paste CSV content.');
        return;
      }
      // Create a File object from the pasted content
      fileToUpload = new File([pasteContent], 'pasted_foods.csv', { type: 'text/csv' });
    }

    if (!fileToUpload) {
      return;
    }

    try {
      const res = await importMutation.mutateAsync(fileToUpload);
      setImportResult(res);
      if (onSuccess && res.insertedCount > 0) {
        onSuccess();
      }
    } catch (err) {
      // Handled by hook mutation onError
    }
  };

  const handleClose = () => {
    setFileList([]);
    setPasteContent('');
    setCsvPreview([]);
    setImportResult(null);
    onCancel();
  };

  return (
    <Modal
      title="Bulk Import Foods"
      open={visible}
      onCancel={handleClose}
      width={700}
      footer={[
        <Button key="close" onClick={handleClose}>
          Close
        </Button>,
        !importResult && (
          <Button
            key="import"
            type="primary"
            onClick={handleImport}
            loading={importMutation.isPending}
            style={{ backgroundColor: 'var(--color-navy)', color: '#fff', border: 'none' }}
          >
            Start Import
          </Button>
        ),
      ]}
      className="bulk-import-modal"
    >
      {!importResult ? (
        <>
          <Tabs activeKey={activeTab} onChange={handleTabChange} className="bulk-import-modal__tabs">
            <Tabs.TabPane tab="Upload CSV File" key="file">
              <Upload.Dragger
                accept=".csv"
                beforeUpload={beforeUpload}
                fileList={fileList}
                onRemove={() => {
                  setFileList([]);
                  setCsvPreview([]);
                }}
                className="bulk-import-modal__dragger"
              >
                <p className="ant-upload-drag-icon">
                  <span className="material-symbols-outlined" style={{ fontSize: 48, color: 'var(--color-navy)' }}>
                    cloud_upload
                  </span>
                </p>
                <p className="ant-upload-text">Click or drag CSV file to this area to upload</p>
                <p className="ant-upload-hint">Support for a single .csv file (Max 10MB)</p>
              </Upload.Dragger>
            </Tabs.TabPane>
            <Tabs.TabPane tab="Paste Raw CSV Text" key="paste">
              <Input.TextArea
                rows={8}
                value={pasteContent}
                onChange={handlePasteChange}
                placeholder="Name,Category,CaloriesPer100g,ProteinPer100g,CarbsPer100g,FatPer100g,FiberPer100g,State&#10;Chicken Breast,Protein,165,31,0,3.6,0,Raw&#10;Brown Rice,Carbs,365,7.1,80,0.7,0.4,Dry"
                className="bulk-import-modal__paste-area font-data"
              />
            </Tabs.TabPane>
          </Tabs>

          {csvPreview.length > 0 && (
            <div className="bulk-import-modal__preview">
              <h3>Preview (First 5 rows):</h3>
              <div className="bulk-import-modal__preview-table-wrapper">
                <table className="bulk-import-modal__preview-table font-data">
                  <thead>
                    <tr>
                      {csvPreview[0].map((h, i) => (
                        <th key={i}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {csvPreview.slice(1).map((row, rowIndex) => (
                      <tr key={rowIndex}>
                        {row.map((cell, cellIndex) => (
                          <td key={cellIndex}>{cell}</td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      ) : (
        <div className="bulk-import-modal__result">
          <Alert
            message="Import Completed"
            description={
              <div>
                <p>Foods Successfully Imported: <strong>{importResult.insertedCount}</strong></p>
                <p>Foods Skipped (Invalid): <strong>{importResult.skippedCount}</strong></p>
              </div>
            }
            type={importResult.skippedCount > 0 ? 'warning' : 'success'}
            showIcon
            style={{ marginBottom: varSpaceSm }}
          />

          {importResult.errors && importResult.errors.length > 0 && (
            <div className="bulk-import-modal__errors-list">
              <h4>Rejected Rows Log:</h4>
              <List
                size="small"
                bordered
                dataSource={importResult.errors}
                renderItem={(item) => (
                  <List.Item className="bulk-import-modal__error-item font-data">
                    <span className="material-symbols-outlined text-danger">cancel</span>
                    {item}
                  </List.Item>
                )}
              />
            </div>
          )}
        </div>
      )}
    </Modal>
  );
};

// Simple helper to avoid parsing error for CSS var references in TSX files
const varSpaceSm = 16;

export default BulkImportModal;
