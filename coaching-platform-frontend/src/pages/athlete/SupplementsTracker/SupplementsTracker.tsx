import React from 'react';
import { Skeleton, Checkbox } from 'antd';
import type { CheckboxChangeEvent } from 'antd/es/checkbox';
import { useTranslation } from 'react-i18next';
import { useGetSupplements, useToggleSupplement } from '../../../hooks/useSupplements/useSupplements';
import { SupplementType, type SupplementDto } from '../../../types/Supplement';
import { getTodayIso, parseUtcDate } from '../../../utils/date';
import './SupplementsTracker.scss';

// ── Supplement Row ────────────────────────────────────────────────────────────
interface SupplementRowProps {
  supplement: SupplementDto;
  onToggle: (id: number) => void;
  isLoading: boolean;
}

const SupplementRow: React.FC<SupplementRowProps> = ({ supplement, onToggle, isLoading }) => {
  const { t, i18n } = useTranslation(['athlete']);
  const handleChange = (e: CheckboxChangeEvent) => {
    if (!isLoading) onToggle(supplement.id);
  };

  const takenAtFormatted = supplement.takenAt
    ? parseUtcDate(supplement.takenAt).toLocaleTimeString(i18n.language, {
        hour: '2-digit',
        minute: '2-digit',
      })
    : null;

  return (
    <div
      className={`supplement-row ${supplement.isTakenToday ? 'supplement-row--taken' : ''}`}
      id={`supplement-row-${supplement.id}`}
    >
      <Checkbox
        checked={supplement.isTakenToday}
        onChange={handleChange}
        disabled={isLoading}
        className="supplement-row__checkbox"
        id={`supplement-checkbox-${supplement.id}`}
      />

      <div className="supplement-row__info">
        <span className="supplement-row__name">{supplement.name}</span>
        {supplement.dosage && (
          <span className="supplement-row__dosage mono">{supplement.dosage}</span>
        )}
        {supplement.notes && (
          <span className="supplement-row__notes">{supplement.notes}</span>
        )}
      </div>

      <div className="supplement-row__status">
        {supplement.isTakenToday ? (
          <span className="supplement-row__taken-badge">
            <span className="material-symbols-outlined">check_circle</span>
            {takenAtFormatted && <span className="mono">{takenAtFormatted}</span>}
          </span>
        ) : (
          <span className="supplement-row__pending-badge">
            <span className="material-symbols-outlined">radio_button_unchecked</span>
            {t('athlete:supplementsTracker.pending')}
          </span>
        )}
      </div>
    </div>
  );
};

// ── Main Page ─────────────────────────────────────────────────────────────────
const SupplementsTracker: React.FC = () => {
  const { t } = useTranslation(['athlete']);
  const today = getTodayIso();
  const { data: supplements, isLoading } = useGetSupplements();
  const { mutate: toggleSupplement, isPending } = useToggleSupplement();

  const handleToggle = (supplementId: number) => {
    toggleSupplement({ supplementScheduleId: supplementId, date: today });
  };

  const essential = supplements?.filter((s) => s.type === SupplementType.Essential) ?? [];
  const optional = supplements?.filter((s) => s.type === SupplementType.Optional) ?? [];
  const totalTaken = supplements?.filter((s) => s.isTakenToday).length ?? 0;
  const total = supplements?.length ?? 0;
  const allDone = total > 0 && totalTaken === total;

  return (
    <div id="supplements-tracker-page" className="supplements-tracker animate-fade-in">
      {/* ── Header ── */}
      <div className="supplements-tracker__header">
        <div>
          <h1 className="supplements-tracker__title">{t('athlete:supplementsTracker.title')}</h1>
          <p className="supplements-tracker__sub">{t('athlete:supplementsTracker.sub')}</p>
        </div>
        {!isLoading && total > 0 && (
          <div className={`supplements-tracker__progress-badge ${allDone ? 'supplements-tracker__progress-badge--done' : ''}`}>
            {allDone ? (
              <>
                <span className="material-symbols-outlined">celebration</span>
                {t('athlete:supplementsTracker.allDone')}
              </>
            ) : (
              <>
                <span className="material-symbols-outlined">pill</span>
                <span className="mono">{totalTaken} / {total}</span>
                {' '}{t('athlete:supplementsTracker.taken')}
              </>
            )}
          </div>
        )}
      </div>

      {/* ── Progress Bar ── */}
      {!isLoading && total > 0 && (
        <div className="supplements-tracker__progress-card">
          <div className="supplements-tracker__progress-bar-wrap">
            <div
              className="supplements-tracker__progress-bar-fill"
              style={{ width: `${(totalTaken / total) * 100}%` }}
            />
          </div>
          <span className="mono supplements-tracker__progress-label">
            {Math.round((totalTaken / total) * 100)}% {t('athlete:supplementsTracker.complete')}
          </span>
        </div>
      )}

      {/* ── Loading Skeleton ── */}
      {isLoading && (
        <div className="supplements-tracker__skeleton">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="supplements-tracker__skeleton-row">
              <Skeleton.Avatar size="small" active shape="square" />
              <Skeleton active paragraph={{ rows: 1, width: ['60%'] }} title={false} />
            </div>
          ))}
        </div>
      )}

      {/* ── Essential Supplements ── */}
      {!isLoading && essential.length > 0 && (
        <div className="supplements-tracker__section">
          <div className="supplements-tracker__section-header supplements-tracker__section-header--essential">
            <span className="material-symbols-outlined">verified</span>
            <h2>{t('athlete:supplementsTracker.essential')}</h2>
            <span className="mono supplements-tracker__section-count">
              {essential.filter((s) => s.isTakenToday).length} / {essential.length}
            </span>
          </div>
          <div className="supplements-tracker__list">
            {essential.map((s) => (
              <SupplementRow
                key={s.id}
                supplement={s}
                onToggle={handleToggle}
                isLoading={isPending}
              />
            ))}
          </div>
        </div>
      )}

      {/* ── Optional Supplements ── */}
      {!isLoading && optional.length > 0 && (
        <div className="supplements-tracker__section">
          <div className="supplements-tracker__section-header supplements-tracker__section-header--optional">
            <span className="material-symbols-outlined">help</span>
            <h2>{t('athlete:supplementsTracker.optional')}</h2>
            <span className="mono supplements-tracker__section-count">
              {optional.filter((s) => s.isTakenToday).length} / {optional.length}
            </span>
          </div>
          <div className="supplements-tracker__list">
            {optional.map((s) => (
              <SupplementRow
                key={s.id}
                supplement={s}
                onToggle={handleToggle}
                isLoading={isPending}
              />
            ))}
          </div>
        </div>
      )}

      {/* ── Empty state ── */}
      {!isLoading && total === 0 && (
        <div className="supplements-tracker__empty">
          <span className="material-symbols-outlined">medication</span>
          <h2>{t('athlete:supplementsTracker.noSupps')}</h2>
          <p>{t('athlete:supplementsTracker.noSuppsDesc')}</p>
        </div>
      )}
    </div>
  );
};

export default SupplementsTracker;
