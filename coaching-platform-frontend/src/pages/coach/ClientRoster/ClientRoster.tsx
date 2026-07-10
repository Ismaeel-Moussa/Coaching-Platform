import React, { useState } from 'react';
import { Table, Input, Tag, Avatar, Button, Pagination } from 'antd';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useGetRoster } from '../../../hooks/useCoachHub/useCoachHub';
import { formatRelativeTime } from '../../../utils/date';
import './ClientRoster.scss';

type ActiveTab = 'All' | 'ComplianceAlert' | 'NoRecentCheckIn';

const ClientRoster: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const initialFilter = searchParams.get('filter') as ActiveTab || 'All';
  const [activeTab, setActiveTab] = useState<ActiveTab>(() => {
    if (initialFilter === 'ComplianceAlert' || initialFilter === 'NoRecentCheckIn') {
      return initialFilter;
    }
    return 'All';
  });

  const [searchQuery, setSearchQuery] = useState<string>('');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const PAGE_SIZE = 10;

  // Convert tab selection to API filter parameter
  const apiFilter = activeTab === 'All' ? undefined : activeTab;

  const { data, isLoading, error } = useGetRoster(currentPage, PAGE_SIZE, apiFilter);

  const handleRowClick = (athleteId: number) => {
    navigate(`/coach/roster/${athleteId}`);
  };

  const getInitials = (name: string) => {
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }
    return parts[0] ? parts[0][0].toUpperCase() : '';
  };

  // Status badge config
  const getStatusTag = (status: string) => {
    switch (status) {
      case 'Active':
        return <Tag color="success" className="roster-tag">Active</Tag>;
      case 'ComplianceAlert':
        return <Tag color="error" className="roster-tag">Compliance Alert</Tag>;
      case 'NoRecentCheckIn':
        return <Tag color="warning" className="roster-tag">No Recent Check-In</Tag>;
      default:
        return <Tag color="default" className="roster-tag">{status}</Tag>;
    }
  };

  // Client-side search filtering
  const rawItems = data?.items ?? [];
  const filteredItems = rawItems.filter((item) =>
    item.athleteName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const columns = [
    {
      title: 'Athlete',
      dataIndex: 'athleteName',
      key: 'athleteName',
      render: (_text: string, record: any) => (
        <div className="roster-table__athlete-cell">
          {record.athleteAvatarUrl ? (
            <Avatar src={record.athleteAvatarUrl} size="large" />
          ) : (
            <Avatar size="large" style={{ backgroundColor: 'var(--surface-container-high)', color: 'var(--color-navy)', fontWeight: 600 }}>
              {getInitials(record.athleteName)}
            </Avatar>
          )}
          <span className="roster-table__athlete-name">{record.athleteName}</span>
        </div>
      ),
    },
    {
      title: 'Active Program',
      dataIndex: 'activeProgramName',
      key: 'activeProgramName',
      render: (programName: string | null) => (
        <span className="roster-table__program-text">
          {programName || <span className="roster-table__program-text--none">None Assigned</span>}
        </span>
      ),
    },
    {
      title: 'Macro Compliance',
      dataIndex: 'macroCompliancePercent',
      key: 'macroCompliancePercent',
      render: (percent: number | null | undefined) => {
        const hasNoData = percent == null;
        const safePercent = percent ?? 0;
        const isAlert = !hasNoData && (safePercent > 105 || safePercent < 40);
        return (
          <div className="roster-table__compliance">
            <div className="roster-table__compliance-bar-track">
              {!hasNoData && safePercent > 0 && (
                <div 
                  className={`roster-table__compliance-bar-fill ${isAlert ? 'roster-table__compliance-bar-fill--alert' : ''}`}
                  style={{ width: `${Math.min(safePercent, 100)}%` }}
                />
              )}
            </div>
            <span className={`roster-table__compliance-text mono ${isAlert ? 'roster-table__compliance-text--alert' : ''}`}>
              {hasNoData ? '—' : `${Math.round(safePercent)}%`}
            </span>
          </div>
        );
      },
    },
    {
      title: 'Last Check-In',
      dataIndex: 'lastCheckInDate',
      key: 'lastCheckInDate',
      render: (dateStr: string | null) => (
        <span className="roster-table__date-text mono">
          {dateStr ? formatRelativeTime(dateStr) : 'Never'}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => getStatusTag(status),
    },
    {
      title: 'Action',
      key: 'action',
      render: (_: any, record: any) => (
        <Button 
          type="link" 
          onClick={(e) => {
            e.stopPropagation();
            handleRowClick(record.athleteId);
          }}
          className="roster-table__action-btn"
        >
          View Profile
        </Button>
      ),
    },
  ];

  return (
    <div id="client-roster-page" className="client-roster animate-fade-in">
      {/* Page Header */}
      <div className="client-roster__header">
        <div>
          <h1 className="client-roster__title">Client Roster</h1>
          <p className="client-roster__subtitle">Manage and track your assigned athletes</p>
        </div>
      </div>

      {/* Roster Controls: Search & Tabs */}
      <div className="client-roster__controls">
        <div className="client-roster__search-wrapper">
          <span className="material-symbols-outlined client-roster__search-icon">search</span>
          <Input
            placeholder="Search by athlete name..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="client-roster__search-input"
            allowClear
          />
        </div>

        <div className="client-roster__tabs">
          <button
            className={`client-roster__tab ${activeTab === 'All' ? 'client-roster__tab--active' : ''}`}
            onClick={() => {
              setActiveTab('All');
              setCurrentPage(1);
              setSearchParams({});
            }}
          >
            All Clients
          </button>
          <button
            className={`client-roster__tab ${activeTab === 'ComplianceAlert' ? 'client-roster__tab--active' : ''}`}
            onClick={() => {
              setActiveTab('ComplianceAlert');
              setCurrentPage(1);
              setSearchParams({ filter: 'ComplianceAlert' });
            }}
          >
            Compliance Alerts
          </button>
          <button
            className={`client-roster__tab ${activeTab === 'NoRecentCheckIn' ? 'client-roster__tab--active' : ''}`}
            onClick={() => {
              setActiveTab('NoRecentCheckIn');
              setCurrentPage(1);
              setSearchParams({ filter: 'NoRecentCheckIn' });
            }}
          >
            No Recent Check-in
          </button>
        </div>
      </div>

      {/* Roster Table */}
      <div className="client-roster__table-container">
        {error ? (
          <div className="client-roster__error">
            <span className="material-symbols-outlined">error_outline</span>
            <p>Failed to load client roster. Please refresh the page.</p>
          </div>
        ) : (
          <>
            <Table
              dataSource={filteredItems}
              columns={columns}
              rowKey="athleteId"
              loading={isLoading}
              pagination={false}
              onRow={(record) => ({
                onClick: () => handleRowClick(record.athleteId),
              })}
              className="roster-table client-roster__desktop-table"
            />
            <div className="client-roster__mobile-cards">
              {filteredItems.map((item) => (
                <div
                  key={item.athleteId}
                  className="client-roster__card-item"
                  onClick={() => handleRowClick(item.athleteId)}
                >
                  <div className="client-roster__card-header">
                    <div className="client-roster__card-athlete">
                      {item.athleteAvatarUrl ? (
                        <Avatar src={item.athleteAvatarUrl} size="large" />
                      ) : (
                        <Avatar size="large" style={{ backgroundColor: 'var(--surface-container-high)', color: 'var(--color-navy)', fontWeight: 600 }}>
                          {getInitials(item.athleteName)}
                        </Avatar>
                      )}
                      <span className="client-roster__card-name">{item.athleteName}</span>
                    </div>
                    {getStatusTag(item.status)}
                  </div>
                  <div className="client-roster__card-body">
                    <div className="client-roster__card-row">
                      <span className="label">Program</span>
                      <span className="value">
                        {item.activeProgramName || <span className="none">None Assigned</span>}
                      </span>
                    </div>
                    <div className="client-roster__card-row">
                      <span className="label">Last Check-in</span>
                      <span className="value mono">
                        {item.lastCheckInDate ? formatRelativeTime(item.lastCheckInDate) : 'Never'}
                      </span>
                    </div>
                    <div className="client-roster__card-row client-roster__card-row--compliance">
                      <span className="label">Compliance</span>
                      <div className="roster-mobile-compliance__wrapper">
                        <div className="roster-mobile-compliance__track">
                          {item.macroCompliancePercent != null && item.macroCompliancePercent > 0 && (
                            <div
                              className={`roster-mobile-compliance__fill ${item.macroCompliancePercent != null && (item.macroCompliancePercent > 105 || item.macroCompliancePercent < 40) ? 'roster-mobile-compliance__fill--alert' : ''}`}
                              style={{ width: `${Math.min(item.macroCompliancePercent, 100)}%` }}
                            />
                          )}
                        </div>
                        <span className={`roster-mobile-compliance__text mono ${item.macroCompliancePercent != null && (item.macroCompliancePercent > 105 || item.macroCompliancePercent < 40) ? 'roster-mobile-compliance__text--alert' : ''}`}>
                          {item.macroCompliancePercent == null ? '—' : `${Math.round(item.macroCompliancePercent)}%`}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="client-roster__card-footer">
                    <Button type="link" className="view-profile-link">
                      View Profile
                      <span className="material-symbols-outlined" style={{ fontSize: 16 }}>chevron_right</span>
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Pagination Controls */}
      {data && data.totalCount > PAGE_SIZE && (
        <div className="client-roster__pagination">
          <Pagination
            current={currentPage}
            pageSize={PAGE_SIZE}
            total={data.totalCount}
            onChange={(page) => setCurrentPage(page)}
            showSizeChanger={false}
          />
        </div>
      )}
    </div>
  );
};

export default ClientRoster;
