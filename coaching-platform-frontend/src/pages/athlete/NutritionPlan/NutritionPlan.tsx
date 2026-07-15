import React from 'react';
import { Alert, Card, Empty, Skeleton, Tag, Typography } from 'antd';
import { useTranslation } from 'react-i18next';
import { useMyNutritionPlan } from '../../../hooks/useNutritionPlans/useNutritionPlans';
import type { NutritionOptionItem } from '../../../types/NutritionPlan';
import './NutritionPlan.scss';

const { Title, Text, Paragraph } = Typography;

const NutritionPlan: React.FC = () => {
  const { i18n } = useTranslation();
  const ar = i18n.resolvedLanguage === 'ar';
  const { data: assignment, isLoading, isError } = useMyNutritionPlan();
  const copy = ar ? {
    title: 'خطتي الغذائية', subtitle: 'الخطة الغذائية التي عيّنها لك المدرب', noPlan: 'لم يتم تعيين خطة غذائية لك حتى الآن.',
    calories: 'سعرة حرارية', protein: 'غ بروتين على الأقل', assigned: 'تاريخ التعيين', coachNotes: 'ملاحظات المدرب',
    choose: 'اختر خياراً واحداً', quantity: 'الكمية', rules: 'قواعد الخطة', error: 'تعذر تحميل خطتك الغذائية. حاول مرة أخرى.',
    training: 'يوم التدريب', rest: 'يوم الراحة', option: 'خيار',
  } : {
    title: 'My nutrition plan', subtitle: 'The nutrition plan assigned by your coach', noPlan: 'No nutrition plan has been assigned to you yet.',
    calories: 'kcal', protein: 'g minimum protein', assigned: 'Assigned', coachNotes: 'Coach notes', choose: 'Choose one option',
    quantity: 'Quantity', rules: 'Plan rules', error: 'We could not load your nutrition plan. Please try again.', training: 'Training day',
    rest: 'Rest day', option: 'Option',
  };

  if (isLoading) return <div className="athlete-plan"><Skeleton active paragraph={{ rows: 10 }} /></div>;
  if (isError) return <div className="athlete-plan"><Alert type="error" showIcon message={copy.error} /></div>;
  if (!assignment) return <div className="athlete-plan athlete-plan--empty"><Empty description={copy.noPlan} /></div>;

  const { plan } = assignment;
  const itemName = (item: NutritionOptionItem) => {
    if (item.foodName) return ar ? item.foodNameAr || item.foodName : item.foodName;
    if (item.recipeName) return ar ? item.recipeNameAr || item.recipeName : item.recipeName;
    return ar ? item.itemNameAr || item.itemName : item.itemName || item.itemNameAr;
  };
  const unitLabel = (unit: NutritionOptionItem['unit']) => {
    if (!ar) return unit;
    return ({
      Gram: 'غ', Milliliter: 'مل', Piece: 'قطعة', Tablespoon: 'ملعقة كبيرة',
      Teaspoon: 'ملعقة صغيرة', Cup: 'كوب', Scoop: 'مكيال',
    } as Record<NutritionOptionItem['unit'], string>)[unit];
  };

  return <div className="athlete-plan">
    <header className="athlete-plan__hero">
      <div>
        <Text className="athlete-plan__eyebrow">{copy.subtitle}</Text>
        <Title>{ar ? plan.nameAr || plan.name : plan.name}</Title>
        {(ar ? plan.descriptionAr || plan.description : plan.description) && <Paragraph>{ar ? plan.descriptionAr || plan.description : plan.description}</Paragraph>}
      </div>
      <div className="athlete-plan__targets">
        <div><strong>{plan.targetCalories}</strong><span>{copy.calories}</span></div>
        <div><strong>{plan.minimumProteinGrams}</strong><span>{copy.protein}</span></div>
      </div>
    </header>

    <div className="athlete-plan__meta">{copy.assigned}: {new Date(assignment.assignedAt).toLocaleDateString(ar ? 'ar-EG' : 'en-US')}</div>
    {assignment.notes && <Alert className="athlete-plan__notes" type="info" showIcon message={copy.coachNotes} description={assignment.notes} />}

    <main className="athlete-plan__meals">
      {plan.mealBlocks.map((block) => <Card key={block.id ?? block.orderIndex} className="athlete-plan__meal">
        <div className="athlete-plan__meal-header">
          <div><span className="material-symbols-outlined">restaurant</span><Title level={3}>{ar ? block.labelAr || block.label : block.label}</Title></div>
          <div className="athlete-plan__meal-tags">
            {block.targetCalories != null && <Tag color="gold">{block.targetCalories} {copy.calories}</Tag>}
            {block.trainingDayOnly && <Tag color="blue">{copy.training}</Tag>}
            {block.restDayOnly && <Tag color="purple">{copy.rest}</Tag>}
          </div>
        </div>
        {(ar ? block.instructionsAr || block.instructions : block.instructions) && <Paragraph className="athlete-plan__instructions">{ar ? block.instructionsAr || block.instructions : block.instructions}</Paragraph>}
        <Text type="secondary">{copy.choose}</Text>
        <div className="athlete-plan__options">
          {block.options.map((option, index) => <section key={option.id ?? option.orderIndex} className="athlete-plan__option">
            <h4><span>{copy.option} {index + 1}</span>{ar ? option.labelAr || option.label : option.label}</h4>
            <ul>{option.items.map((item) => <li key={item.id ?? item.orderIndex}>
              <span>{itemName(item)}</span>
              <strong>{item.quantity} {unitLabel(item.unit)}</strong>
            </li>)}</ul>
          </section>)}
        </div>
      </Card>)}
    </main>

    {!!plan.rules.length && <Card className="athlete-plan__rules" title={copy.rules}>
      <ol>{plan.rules.map(rule => <li key={rule.id ?? rule.orderIndex}>{ar ? rule.textAr || rule.text : rule.text || rule.textAr}</li>)}</ol>
    </Card>}
  </div>;
};

export default NutritionPlan;
