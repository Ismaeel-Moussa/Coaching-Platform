import React, { useState, useEffect } from 'react';
import { 
  Select, Card, Button, InputNumber, Input, 
  Modal, Skeleton, Avatar, Tooltip 
} from 'antd';
import { useTranslation } from 'react-i18next';
import { useGetRoster, useGetAthleteProfile } from '../../../hooks/useCoachHub/useCoachHub';
import { useGetWorkoutTemplates, useAssignTemplate } from '../../../hooks/useWorkoutTemplates/useWorkoutTemplates';
import { 
  useSetMacroTargets, 
  useGetAthleteSupplements, 
  useAddAthleteSupplement, 
  useUpdateAthleteSupplement, 
  useDeleteAthleteSupplement 
} from '../../../hooks/useAthleteAssignment/useAthleteAssignment';
import type { SupplementDto } from '../../../types/Supplement';
import './AthleteAssignmentHub.scss';

const { Option } = Select;

const AthleteAssignmentHub: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const [selectedAthleteId, setSelectedAthleteId] = useState<number | null>(null);

  // Roster and Templates lists
  const { data: rosterData, isLoading: isRosterLoading } = useGetRoster(1, 100);
  const { data: templatesData, isLoading: isTemplatesLoading } = useGetWorkoutTemplates({ page: 1, pageSize: 100 });

  // Selected athlete detail query
  const { data: profile, isLoading: isProfileLoading } = useGetAthleteProfile(selectedAthleteId!);
  const { data: supplements, isLoading: isSupplementsLoading } = useGetAthleteSupplements(selectedAthleteId!);

  // Mutations
  const setMacroTargetsMutation = useSetMacroTargets(selectedAthleteId!);
  const assignTemplateMutation = useAssignTemplate();
  const addSupplementMutation = useAddAthleteSupplement(selectedAthleteId!);
  const updateSupplementMutation = useUpdateAthleteSupplement(selectedAthleteId!);
  const deleteSupplementMutation = useDeleteAthleteSupplement(selectedAthleteId!);

  // Program Assign modal state
  const [isProgramModalVisible, setIsProgramModalVisible] = useState(false);
  const [tempSelectedProgramId, setTempSelectedProgramId] = useState<number | null>(null);

  // Supplements modal state
  const [isSupplementModalVisible, setIsSupplementModalVisible] = useState(false);
  const [editingSupplement, setEditingSupplement] = useState<SupplementDto | null>(null);
  
  // Supplements Form State
  const [supName, setSupName] = useState('');
  const [supType, setSupType] = useState<'Essential' | 'Optional'>('Essential');
  const [supDosage, setSupDosage] = useState('');
  const [supNotes, setSupNotes] = useState('');

  // Nutrition targets form state
  const [calories, setCalories] = useState<number | null>(null);
  const [protein, setProtein] = useState<number | null>(null);
  const [carbs, setCarbs] = useState<number | null>(null);
  const [fat, setFat] = useState<number | null>(null);
  const [water, setWater] = useState<number | null>(null);
  const [steps, setSteps] = useState<number | null>(null);

  // Synced targets form state when profile updates
  useEffect(() => {
    if (profile?.currentTargets) {
      setCalories(Number(profile.currentTargets.targetCalories));
      setProtein(Number(profile.currentTargets.targetProtein));
      setCarbs(Number(profile.currentTargets.targetCarbs));
      setFat(Number(profile.currentTargets.targetFat));
      setWater(Number(profile.currentTargets.waterLitersTarget));
      setSteps(profile.currentTargets.stepsTarget);
    } else {
      setCalories(2000);
      setProtein(150);
      setCarbs(200);
      setFat(65);
      setWater(4);
      setSteps(7000);
    }
  }, [profile]);

  // Handle selected athlete changes
  const handleAthleteChange = (value: number) => {
    setSelectedAthleteId(value);
  };

  // Find roster info for active program display
  const selectedRosterItem = rosterData?.items?.find((item: any) => item.athleteId === selectedAthleteId);

  // Save Nutrition Targets
  const handleSaveNutrition = () => {
    if (
      calories === null || 
      protein === null || 
      carbs === null || 
      fat === null || 
      water === null || 
      steps === null
    ) {
      Modal.error({ title: t('common:actions.confirm'), content: t('coach:assignmentHub.requiredFieldsDesc', { defaultValue: 'All daily target fields are required.' }) });
      return;
    }

    setMacroTargetsMutation.mutate({
      targetCalories: calories,
      targetProtein: protein,
      targetCarbs: carbs,
      targetFat: fat,
      waterLitersTarget: water,
      stepsTarget: steps
    });
  };

  // Assign program template
  const handleAssignProgram = () => {
    if (!tempSelectedProgramId || !selectedAthleteId) {
      Modal.error({ title: t('common:actions.confirm'), content: t('coach:assignmentHub.selectTemplate') });
      return;
    }

    assignTemplateMutation.mutate({
      id: tempSelectedProgramId,
      form: { athleteIds: [selectedAthleteId] }
    }, {
      onSuccess: () => {
        setIsProgramModalVisible(false);
        setTempSelectedProgramId(null);
      }
    });
  };

  // Supplement Form Open (Add Mode)
  const openAddSupplement = () => {
    setEditingSupplement(null);
    setSupName('');
    setSupType('Essential');
    setSupDosage('');
    setSupNotes('');
    setIsSupplementModalVisible(true);
  };

  // Supplement Form Open (Edit Mode)
  const openEditSupplement = (sup: SupplementDto) => {
    setEditingSupplement(sup);
    setSupName(sup.name);
    setSupType(sup.type as 'Essential' | 'Optional');
    setSupDosage(sup.dosage || '');
    setSupNotes(sup.notes || '');
    setIsSupplementModalVisible(true);
  };

  // Save Supplement (Add or Edit)
  const handleSaveSupplement = () => {
    if (!supName.trim()) {
      Modal.error({ title: t('common:actions.confirm'), content: t('coach:assignmentHub.suppNameRequired', { defaultValue: 'Supplement name is required.' }) });
      return;
    }

    const payload = {
      name: supName.trim(),
      type: supType,
      dosage: supDosage.trim() || null,
      notes: supNotes.trim() || null
    };

    if (editingSupplement) {
      updateSupplementMutation.mutate({
        id: editingSupplement.id,
        form: payload
      }, {
        onSuccess: () => setIsSupplementModalVisible(false)
      });
    } else {
      addSupplementMutation.mutate(payload, {
        onSuccess: () => setIsSupplementModalVisible(false)
      });
    }
  };

  // Delete Supplement
  const handleDeleteSupplement = (id: number) => {
    Modal.confirm({
      title: t('coach:assignmentHub.deleteSupp'),
      content: t('coach:assignmentHub.deleteConfirm'),
      okText: t('common:actions.delete'),
      okType: 'danger',
      cancelText: t('common:actions.cancel'),
      onOk: () => {
        deleteSupplementMutation.mutate(id);
      }
    });
  };

  return (
    <div id="athlete-hub-page" className="athlete-assignment-hub animate-fade-in">
      
      {/* Page Header */}
      <div className="athlete-assignment-hub__header">
        <h1 className="athlete-assignment-hub__title">{t('coach:assignmentHub.title')}</h1>
        <p className="athlete-assignment-hub__subtitle">
          {t('coach:assignmentHub.subtitle')}
        </p>
      </div>

      {/* Roster Selector Card */}
      <div className="athlete-assignment-hub__selector-card">
        <div className="athlete-assignment-hub__select-group">
          <div className="athlete-assignment-hub__select-icon">
            <span className="material-symbols-outlined">person_search</span>
          </div>
          <div className="athlete-assignment-hub__select-wrapper">
            <Select
              placeholder={t('coach:assignmentHub.searchPlaceholder')}
              onChange={handleAthleteChange}
              value={selectedAthleteId}
              className="athlete-assignment-hub__select"
              loading={isRosterLoading}
              showSearch
              filterOption={(input, option) =>
                String(option?.children ?? '').toLowerCase().includes(input.toLowerCase())
              }
            >
              {rosterData?.items?.map((ath: any) => (
                <Option key={ath.athleteId} value={ath.athleteId}>
                  {ath.athleteName}
                </Option>
              ))}
            </Select>
          </div>
        </div>

        {selectedAthleteId && profile && (
          <>
            <div className="athlete-assignment-hub__divider" />
            <div className="athlete-assignment-hub__selected-athlete-info">
              <div className="athlete-assignment-hub__avatar-wrapper">
                <Avatar 
                  src={profile.avatarUrl || undefined} 
                  style={{ backgroundColor: 'var(--color-gold)', color: 'var(--color-navy)' }}
                >
                  {profile.fullName?.[0]}
                </Avatar>
              </div>
              <div className="athlete-assignment-hub__athlete-meta">
                <div className="athlete-assignment-hub__athlete-name">{profile.fullName}</div>
                <span className="athlete-assignment-hub__athlete-goal">
                  <span className="material-symbols-outlined" style={{ fontSize: 11 }}>flag</span>
                  {profile.targetGoal}
                </span>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Main Hub Work Area */}
      {!selectedAthleteId ? (
        <div className="athlete-assignment-hub__empty-state">
          <div className="empty-icon">
            <span className="material-symbols-outlined">assignment_ind</span>
          </div>
          <h3>{t('coach:assignmentHub.noAthleteSelected')}</h3>
          <p>{t('coach:assignmentHub.noAthleteSelectedDesc')}</p>
        </div>
      ) : isProfileLoading ? (
        <div style={{ backgroundColor: 'var(--color-white)', padding: 28, borderRadius: 'var(--radius-lg)', border: '1px solid var(--color-border-light)' }}>
          <Skeleton active paragraph={{ rows: 8 }} />
        </div>
      ) : (
        <div className="athlete-assignment-hub__grid animate-fade-in">
          
          {/* Left Column: Program Template & Nutrition Targets */}
          <div className="athlete-assignment-hub__column">
            
            {/* Workout Program Template Card */}
            <Card 
              className="athlete-assignment-hub__card"
              title={
                <div className="athlete-assignment-hub__card-header">
                  <div className="icon-bubble">
                    <span className="material-symbols-outlined">fitness_center</span>
                  </div>
                  <h3>{t('coach:assignmentHub.workoutProgram')}</h3>
                </div>
              }
              variant="borderless"
            >
              {selectedRosterItem?.activeProgramName ? (
                <div className="athlete-assignment-hub__current-program">
                  <div className="athlete-assignment-hub__program-info">
                    <span className="program-label">{t('coach:assignmentHub.activeTemplate')}</span>
                    <span className="program-name">{selectedRosterItem.activeProgramName}</span>
                  </div>
                  <Button 
                    type="primary" 
                    onClick={() => setIsProgramModalVisible(true)}
                    icon={<span className="material-symbols-outlined" style={{ fontSize: 15 }}>sync</span>}
                  >
                    {t('coach:assignmentHub.changeBtn')}
                  </Button>
                </div>
              ) : (
                <div>
                  <p className="athlete-assignment-hub__no-program">{t('coach:assignmentHub.noProgram')}</p>
                  <Button 
                    type="primary" 
                    block 
                    onClick={() => setIsProgramModalVisible(true)}
                    icon={<span className="material-symbols-outlined" style={{ fontSize: 15 }}>add</span>}
                  >
                    {t('coach:assignmentHub.assignTemplate')}
                  </Button>
                </div>
              )}
            </Card>

            {/* Daily Nutrition Targets Card */}
            <Card 
              className="athlete-assignment-hub__card"
              title={
                <div className="athlete-assignment-hub__card-header">
                  <div className="icon-bubble">
                    <span className="material-symbols-outlined">nutrition</span>
                  </div>
                  <h3>{t('coach:assignmentHub.nutritionTargets')}</h3>
                </div>
              }
              variant="borderless"
            >
              <div className="athlete-assignment-hub__form">
                <div className="athlete-assignment-hub__macro-grid">
                  <div className="athlete-assignment-hub__macro-field">
                    <span className="athlete-assignment-hub__macro-label athlete-assignment-hub__macro-label--calories">
                      <span className="macro-dot" />
                      {t('coach:assignmentHub.calories')}
                    </span>
                    <InputNumber
                      min={100}
                      max={10000}
                      value={calories}
                      onChange={(val) => setCalories(val)}
                    />
                  </div>
                  <div className="athlete-assignment-hub__macro-field">
                    <span className="athlete-assignment-hub__macro-label athlete-assignment-hub__macro-label--protein">
                      <span className="macro-dot" />
                      {t('coach:assignmentHub.protein')}
                    </span>
                    <InputNumber
                      min={10}
                      max={1000}
                      value={protein}
                      onChange={(val) => setProtein(val)}
                    />
                  </div>
                  <div className="athlete-assignment-hub__macro-field">
                    <span className="athlete-assignment-hub__macro-label athlete-assignment-hub__macro-label--carbs">
                      <span className="macro-dot" />
                      {t('coach:assignmentHub.carbs')}
                    </span>
                    <InputNumber
                      min={10}
                      max={1500}
                      value={carbs}
                      onChange={(val) => setCarbs(val)}
                    />
                  </div>
                  <div className="athlete-assignment-hub__macro-field">
                    <span className="athlete-assignment-hub__macro-label athlete-assignment-hub__macro-label--fat">
                      <span className="macro-dot" />
                      {t('coach:assignmentHub.fat')}
                    </span>
                    <InputNumber
                      min={5}
                      max={500}
                      value={fat}
                      onChange={(val) => setFat(val)}
                    />
                  </div>
                </div>
                <Button 
                  type="primary" 
                  className="athlete-assignment-hub__submit-btn"
                  onClick={handleSaveNutrition}
                  loading={setMacroTargetsMutation.isPending}
                  icon={<span className="material-symbols-outlined" style={{ fontSize: 15 }}>save</span>}
                >
                  {t('coach:assignmentHub.updateNutrition')}
                </Button>
              </div>
            </Card>
          </div>

          {/* Right Column: Water, Steps & Supplements */}
          <div className="athlete-assignment-hub__column">
            
            {/* Daily Water & Steps Targets Card */}
            <Card 
              className="athlete-assignment-hub__card"
              title={
                <div className="athlete-assignment-hub__card-header">
                  <div className="icon-bubble">
                    <span className="material-symbols-outlined">track_changes</span>
                  </div>
                  <h3>{t('coach:assignmentHub.waterSteps')}</h3>
                </div>
              }
              variant="borderless"
            >
              <div className="athlete-assignment-hub__form">
                <div className="athlete-assignment-hub__form-grid">
                  <div className="athlete-assignment-hub__form-field">
                    <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.dailyWater')}</span>
                    <InputNumber
                      min={0.5}
                      max={20}
                      step={0.5}
                      value={water}
                      onChange={(val) => setWater(val)}
                    />
                  </div>
                  <div className="athlete-assignment-hub__form-field">
                    <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.dailySteps')}</span>
                    <InputNumber
                      min={100}
                      max={100000}
                      step={500}
                      value={steps}
                      onChange={(val) => setSteps(val)}
                    />
                  </div>
                </div>
                <Button 
                  type="primary" 
                  className="athlete-assignment-hub__submit-btn"
                  onClick={handleSaveNutrition}
                  loading={setMacroTargetsMutation.isPending}
                  icon={<span className="material-symbols-outlined" style={{ fontSize: 15 }}>save</span>}
                >
                  {t('coach:assignmentHub.updateActivity')}
                </Button>
              </div>
            </Card>

            {/* Supplements Tracker Card */}
            <Card 
              className="athlete-assignment-hub__card"
              title={
                <div className="athlete-assignment-hub__card-header">
                  <div className="icon-bubble">
                    <span className="material-symbols-outlined">medication</span>
                  </div>
                  <h3>{t('coach:assignmentHub.supplementSchedule')}</h3>
                </div>
              }
              variant="borderless"
            >
              <div className="athlete-assignment-hub__form" style={{ gap: 14 }}>
                <Button 
                  type="dashed" 
                  className="athlete-assignment-hub__add-supplement-btn"
                  onClick={openAddSupplement}
                  icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>add</span>}
                >
                  {t('coach:assignmentHub.addSupplement')}
                </Button>

                {isSupplementsLoading ? (
                  <Skeleton active paragraph={{ rows: 3 }} />
                ) : !supplements || supplements.length === 0 ? (
                  <div className="athlete-assignment-hub__no-supplements">
                    <span className="material-symbols-outlined">medication_liquid</span>
                    {t('coach:assignmentHub.noSupplements')}
                  </div>
                ) : (
                  <div className="athlete-assignment-hub__supplements-list">
                    {supplements.map((sup) => (
                      <div key={sup.id} className="athlete-assignment-hub__supplement-item">
                        <div className="athlete-assignment-hub__supplement-info">
                          <div className="sup-name-row">
                            <span className="sup-name">{sup.name}</span>
                            <span className={`sup-type-badge sup-type-badge--${sup.type.toLowerCase()}`}>
                              {sup.type === 'Essential' ? t('coach:assignmentHub.essential') : t('coach:assignmentHub.optional')}
                            </span>
                          </div>
                          {sup.dosage && (
                            <div className="sup-dosage">
                              <span className="material-symbols-outlined">straighten</span>
                              {sup.dosage}
                            </div>
                          )}
                          {sup.notes && (
                            <div className="sup-notes" title={sup.notes}>
                              {sup.notes}
                            </div>
                          )}
                        </div>
                        <div className="athlete-assignment-hub__supplement-actions">
                          <Tooltip title={t('common:actions.edit')}>
                            <Button 
                              type="text" 
                              size="small"
                              onClick={() => openEditSupplement(sup)}
                              icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>edit</span>}
                            />
                          </Tooltip>
                          <Tooltip title={t('common:actions.delete')}>
                            <Button 
                              type="text" 
                              danger 
                              size="small"
                              onClick={() => handleDeleteSupplement(sup.id)}
                              icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span>}
                            />
                          </Tooltip>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>

          </div>

        </div>
      )}

      {/* Program Template Assignment Modal */}
      <Modal
        title={t('coach:assignmentHub.modalAssignTitle')}
        open={isProgramModalVisible}
        onCancel={() => {
          setIsProgramModalVisible(false);
          setTempSelectedProgramId(null);
        }}
        onOk={handleAssignProgram}
        okText={t('coach:assignmentHub.assignTemplate')}
        okButtonProps={{ loading: assignTemplateMutation.isPending }}
        width={450}
      >
        <div style={{ padding: '8px 0' }}>
          <p style={{ fontSize: 13, color: 'var(--color-text-secondary)', marginBottom: 16 }}
             dangerouslySetInnerHTML={{ __html: t('coach:assignmentHub.modalAssignDesc', { name: profile?.fullName }) }}
          />
          <Select
            placeholder={t('coach:assignmentHub.selectTemplate')}
            style={{ width: '100%' }}
            value={tempSelectedProgramId}
            onChange={(val) => setTempSelectedProgramId(val)}
            loading={isTemplatesLoading}
            options={templatesData?.items?.map((tmpl: any) => ({
              value: tmpl.id,
              label: `${tmpl.name} (${tmpl.dayCount} ${t('coach:templateBuilder.daysCount', { count: tmpl.dayCount })})`
            })) || []}
          />
        </div>
      </Modal>

      {/* Supplement Add/Edit Modal */}
      <Modal
        title={editingSupplement ? t('coach:assignmentHub.modalEditSupp') : t('coach:assignmentHub.modalAddSupp')}
        open={isSupplementModalVisible}
        onCancel={() => setIsSupplementModalVisible(false)}
        onOk={handleSaveSupplement}
        okText={editingSupplement ? t('coach:assignmentHub.updateSchedule') : t('coach:assignmentHub.addSchedule')}
        okButtonProps={{ 
          loading: addSupplementMutation.isPending || updateSupplementMutation.isPending 
        }}
        width={450}
      >
        <div className="supplement-modal-form" style={{ padding: '8px 0' }}>
          <div className="athlete-assignment-hub__form-field">
            <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.suppName')}</span>
            <Input 
              placeholder={t('coach:assignmentHub.suppNamePlaceholder', { defaultValue: 'e.g. Creatine Monohydrate' })} 
              value={supName} 
              onChange={(e) => setSupName(e.target.value)}
            />
          </div>
          <div className="athlete-assignment-hub__form-field">
            <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.suppType')}</span>
            <Select 
              value={supType} 
              onChange={(val) => setSupType(val as 'Essential' | 'Optional')}
            >
              <Option value="Essential">{t('coach:assignmentHub.essential')}</Option>
              <Option value="Optional">{t('coach:assignmentHub.optional')}</Option>
            </Select>
          </div>
          <div className="athlete-assignment-hub__form-field">
            <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.suppDosage')}</span>
            <Input 
              placeholder={t('coach:assignmentHub.suppDosagePlaceholder', { defaultValue: 'e.g. 5g daily post-workout' })} 
              value={supDosage} 
              onChange={(e) => setSupDosage(e.target.value)}
            />
          </div>
          <div className="athlete-assignment-hub__form-field">
            <span className="athlete-assignment-hub__field-label">{t('coach:assignmentHub.suppNotes')}</span>
            <Input.TextArea 
              placeholder={t('coach:assignmentHub.suppNotesPlaceholder')} 
              rows={3} 
              value={supNotes} 
              onChange={(e) => setSupNotes(e.target.value)}
            />
          </div>
        </div>
      </Modal>

    </div>
  );
};

export default AthleteAssignmentHub;
