import React, { useState } from 'react';
import { Tabs, Table, Button, Input, Select, Space, Popconfirm, Modal, Form, InputNumber, Empty, Radio, Tag } from 'antd';
import { useTranslation } from 'react-i18next';
import { useSearchFoods, useCreateFood, useUpdateFood, useDeleteFood } from '../../../hooks/useFoods/useFoods';
import { useGetRecipes, useDeleteRecipe } from '../../../hooks/useRecipes/useRecipes';
import BulkImportModal from '../../../components/BulkImportModal/BulkImportModal';
import CreateRecipeModal from '../../../components/CreateRecipeModal/CreateRecipeModal';
import RecipeCard from '../../../components/RecipeCard/RecipeCard';
import type { FoodDto, FoodCategory, CreateFoodForm } from '../../../types/Food';
import type { RecipeDto } from '../../../types/Recipe';
import './FoodRecipeAdmin.scss';

const FOOD_CATEGORIES: FoodCategory[] = ['Protein', 'Carbs', 'Fat', 'Vegetable', 'Dairy', 'Fruit', 'Meat', 'Bakery', 'Cheese', 'Poultry', 'Nuts', 'Oils', 'Condiments', 'Juice', 'Snacks'];

const getFoodCategoryLabel = (category: string, t: any) => {
  const key = `common:foodCategories.${category.toLowerCase()}`;
  return t(key, { defaultValue: category });
};

const FoodRecipeAdmin: React.FC = () => {
  const { t } = useTranslation(['common', 'athlete', 'coach']);
  const [activeTab, setActiveTab] = useState<string>('foods');

  // Foods state
  const [foodSearch, setFoodSearch] = useState<string>('');
  const [foodCategory, setFoodCategory] = useState<FoodCategory | undefined>(undefined);
  const [foodPage, setFoodPage] = useState<number>(1);
  const [foodPageSize, setFoodPageSize] = useState<number>(7);
  const [isFoodModalVisible, setIsFoodModalVisible] = useState<boolean>(false);
  const [editingFood, setEditingFood] = useState<FoodDto | null>(null);
  const [isBulkImportVisible, setIsBulkImportVisible] = useState<boolean>(false);

  // Recipes state
  const [isRecipeModalVisible, setIsRecipeModalVisible] = useState<boolean>(false);
  const [editingRecipe, setEditingRecipe] = useState<RecipeDto | null>(null);

  const [foodForm] = Form.useForm();

  // Queries & Mutations
  const { data: foodsData, isLoading: isFoodsLoading } = useSearchFoods({
    search: foodSearch || undefined,
    category: foodCategory,
    page: foodPage,
    pageSize: foodPageSize,
  });

  const { data: recipesData, isLoading: isRecipesLoading } = useGetRecipes({
    isJokerRecipe: true,
    pageSize: 100,
  });

  const createFoodMutation = useCreateFood();
  const updateFoodMutation = useUpdateFood();
  const deleteFoodMutation = useDeleteFood();
  const deleteRecipeMutation = useDeleteRecipe();

  const FOOD_CATEGORY_OPTIONS = FOOD_CATEGORIES.map((cat) => ({
    label: getFoodCategoryLabel(cat, t),
    value: cat,
  }));

  // Food CRUD handlers
  const handleAddFoodClick = () => {
    setEditingFood(null);
    foodForm.resetFields();
    setIsFoodModalVisible(true);
  };

  const handleEditFoodClick = (food: FoodDto) => {
    setEditingFood(food);
    foodForm.setFieldsValue(food);
    setIsFoodModalVisible(true);
  };

  const handleSaveFood = async () => {
    try {
      const values = await foodForm.validateFields();
      if (editingFood) {
        await updateFoodMutation.mutateAsync({ id: editingFood.id, form: values });
      } else {
        await createFoodMutation.mutateAsync(values);
      }
      setIsFoodModalVisible(false);
    } catch (err) {
      // Form validation handles user feedback
    }
  };

  const handleDeleteFood = (id: number) => {
    deleteFoodMutation.mutate(id);
  };

  // Recipe CRUD handlers
  const handleDeleteRecipe = (id: number) => {
    deleteRecipeMutation.mutate(id);
  };

  const foodColumns = [
    {
      title: t('coach:foodAdmin.table.name'),
      dataIndex: 'name',
      key: 'name',
      render: (text: string) => <strong className="food-table__name">{text}</strong>,
    },
    {
      title: t('coach:foodAdmin.table.category'),
      dataIndex: 'category',
      key: 'category',
      render: (cat: string) => <span className="food-table__category font-data">{getFoodCategoryLabel(cat, t)}</span>,
    },

    {
      title: t('coach:foodAdmin.table.calories'),
      dataIndex: 'caloriesPer100g',
      key: 'caloriesPer100g',
      render: (val: number) => <span className="mono">{Math.round(val)} {t('common:units.kcal')}</span>,
    },
    {
      title: t('coach:foodAdmin.table.protein'),
      dataIndex: 'proteinPer100g',
      key: 'proteinPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: t('coach:foodAdmin.table.carbs'),
      dataIndex: 'carbsPer100g',
      key: 'carbsPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: t('coach:foodAdmin.table.fat'),
      dataIndex: 'fatPer100g',
      key: 'fatPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: t('coach:foodAdmin.table.fiber'),
      dataIndex: 'fiberPer100g',
      key: 'fiberPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: t('coach:foodAdmin.table.actions'),
      key: 'actions',
      render: (_: any, record: FoodDto) => (
        <Space size="middle">
          <Button
            type="text"
            onClick={() => handleEditFoodClick(record)}
            className="food-table__action-btn"
            icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>edit</span>}
          />
          <Popconfirm
            title={t('coach:foodAdmin.deleteFoodConfirm')}
            description={t('coach:foodAdmin.deleteFoodDesc')}
            onConfirm={() => handleDeleteFood(record.id)}
            okText={t('common:confirm.yes')}
            cancelText={t('common:confirm.no')}
            okButtonProps={{ danger: true }}
          >
            <Button
              type="text"
              danger
              className="food-table__action-btn"
              icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>delete</span>}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div id="food-recipe-admin-page" className="food-recipe-admin animate-fade-in">
      <div className="food-recipe-admin__header">
        <div>
          <h1 className="food-recipe-admin__title">{t('coach:foodAdmin.title')}</h1>
          <p className="food-recipe-admin__subtitle">{t('coach:foodAdmin.subtitle')}</p>
        </div>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={(key) => setActiveTab(key)}
        className="food-recipe-admin__tabs"
        items={[
          {
            key: 'foods',
            label: t('coach:foodAdmin.tabFoods'),
            children: (
              <div className="food-recipe-admin__tab-content">
                <div className="food-recipe-admin__controls">
                  <div className="food-recipe-admin__filters">
                    <Input
                      placeholder={t('coach:foodAdmin.searchPlaceholder')}
                      prefix={<span className="material-symbols-outlined" style={{ fontSize: 18, color: 'var(--color-border-strong)' }}>search</span>}
                      value={foodSearch}
                      onChange={(e) => {
                        setFoodSearch(e.target.value);
                        setFoodPage(1);
                      }}
                      allowClear
                      style={{ width: 240 }}
                    />
                    <Select
                      placeholder={t('coach:foodAdmin.categoryPlaceholder')}
                      value={foodCategory}
                      onChange={(val) => {
                        setFoodCategory(val);
                        setFoodPage(1);
                      }}
                      allowClear
                      style={{ width: 160 }}
                      options={FOOD_CATEGORY_OPTIONS}
                    />

                  </div>
                  <Space>
                    <Button
                      type="default"
                      onClick={() => setIsBulkImportVisible(true)}
                      icon={<span className="material-symbols-outlined">upload_file</span>}
                      className="food-recipe-admin__btn"
                    >
                      {t('coach:foodAdmin.bulkImport')}
                    </Button>
                    <Button
                      type="primary"
                      onClick={handleAddFoodClick}
                      icon={<span className="material-symbols-outlined">add</span>}
                      className="food-recipe-admin__btn food-recipe-admin__btn--navy"
                    >
                      {t('coach:foodAdmin.addFood')}
                    </Button>
                  </Space>
                </div>

                <Table
                  dataSource={foodsData?.items || []}
                  columns={foodColumns}
                  rowKey="id"
                  loading={isFoodsLoading}
                  pagination={{
                    current: foodPage,
                    pageSize: foodPageSize,
                    total: foodsData?.totalCount || 0,
                    showSizeChanger: true,
                    pageSizeOptions: ['7', '10', '15', '20'],
                    onChange: (page, pageSize) => {
                      setFoodPage(page);
                      if (pageSize && pageSize !== foodPageSize) {
                        setFoodPageSize(pageSize);
                        setFoodPage(1);
                      }
                    },
                  }}
                  className="food-recipe-admin__table food-recipe-admin__desktop-table"
                />
                <div className="food-recipe-admin__mobile-cards">
                  {foodsData?.items.map((food) => (
                    <div key={food.id} className="food-recipe-admin__card-item">
                      <div className="food-recipe-admin__card-header">
                        <strong className="food-name">{food.name}</strong>
                        <span className="food-category-badge">{getFoodCategoryLabel(food.category, t)}</span>
                      </div>
                      <div className="food-recipe-admin__card-body">
                        <div className="food-recipe-admin__macro-row">
                          <div className="macro-item">
                            <span className="label">{t('common:labels.calories')}</span>
                            <span className="val mono">{Math.round(food.caloriesPer100g)} {t('common:units.kcal')}</span>
                          </div>
                          <div className="macro-item">
                            <span className="label">{t('common:labels.protein')}</span>
                            <span className="val mono">{food.proteinPer100g}g</span>
                          </div>
                          <div className="macro-item">
                            <span className="label">{t('common:labels.carbs')}</span>
                            <span className="val mono">{food.carbsPer100g}g</span>
                          </div>
                          <div className="macro-item">
                            <span className="label">{t('common:labels.fat')}</span>
                            <span className="val mono">{food.fatPer100g}g</span>
                          </div>
                          {food.fiberPer100g > 0 && (
                            <div className="macro-item">
                              <span className="label">{t('common:labels.fiber')}</span>
                              <span className="val mono">{food.fiberPer100g}g</span>
                            </div>
                          )}
                        </div>
                      </div>
                      <div className="food-recipe-admin__card-footer">
                        <Space size="middle">
                          <Button
                            type="primary"
                            size="small"
                            onClick={() => handleEditFoodClick(food)}
                            icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>edit</span>}
                            className="edit-btn"
                          >
                            {t('common:actions.edit')}
                          </Button>
                          <Popconfirm
                            title={t('coach:foodAdmin.deleteFoodConfirm')}
                            description={t('coach:foodAdmin.deleteFoodDesc')}
                            onConfirm={() => handleDeleteFood(food.id)}
                            okText={t('common:confirm.yes')}
                            cancelText={t('common:confirm.no')}
                            okButtonProps={{ danger: true }}
                          >
                            <Button
                              danger
                              size="small"
                              icon={<span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span>}
                            >
                              {t('common:actions.delete')}
                            </Button>
                          </Popconfirm>
                        </Space>
                      </div>
                    </div>
                  ))}
                  {/* Inline Pagination for Mobile */}
                  {foodsData && foodsData.totalCount > foodPageSize && (
                    <div className="food-recipe-admin__mobile-pagination">
                      <Button
                        disabled={foodPage === 1}
                        onClick={() => setFoodPage((p) => p - 1)}
                        size="small"
                      >
                        {t('common:pagination.prev')}
                      </Button>
                      <span className="pagination-text">
                        {t('common:pagination.pageOf', { page: foodPage, total: Math.ceil(foodsData.totalCount / foodPageSize) })}
                      </span>
                      <Button
                        disabled={foodPage * foodPageSize >= foodsData.totalCount}
                        onClick={() => setFoodPage((p) => p + 1)}
                        size="small"
                      >
                        {t('common:pagination.next')}
                      </Button>
                    </div>
                  )}
                </div>
              </div>
            ),
          },
          {
            key: 'recipes',
            label: t('coach:foodAdmin.tabCoachRecipes'),
            children: (
              <div className="food-recipe-admin__tab-content">
                <div className="food-recipe-admin__controls">
                  <Button
                    type="primary"
                    onClick={() => {
                      setEditingRecipe(null);
                      setIsRecipeModalVisible(true);
                    }}
                    icon={<span className="material-symbols-outlined">restaurant_menu</span>}
                    className="food-recipe-admin__btn food-recipe-admin__btn--navy"
                  >
                    {t('coach:foodAdmin.createRecipe')}
                  </Button>
                </div>

                {isRecipesLoading ? (
                  <div className="food-recipe-admin__recipe-grid">
                    {[1, 2, 3].map((i) => (
                      <div key={i} className="recipe-skeleton" style={{ height: 260, background: '#fff', borderRadius: 8, border: '1px solid var(--color-border)' }} />
                    ))}
                  </div>
                ) : recipesData?.items.length === 0 ? (
                  <Empty description={t('coach:foodAdmin.noRecipes')} style={{ padding: '60px 0' }} />
                ) : (
                  <div className="food-recipe-admin__recipe-grid">
                    {recipesData?.items.map((recipe) => (
                      <div key={recipe.id} className="food-recipe-admin__recipe-card-wrapper">
                        <RecipeCard recipe={recipe} />
                        <div className="food-recipe-admin__recipe-overlay-actions">
                          <Button
                            type="primary"
                            shape="circle"
                            icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>edit</span>}
                            onClick={() => {
                              setEditingRecipe(recipe);
                              setIsRecipeModalVisible(true);
                            }}
                            style={{ backgroundColor: 'var(--color-gold)', borderColor: 'var(--color-gold)', color: 'var(--color-navy)' }}
                          />
                          <Popconfirm
                            title={t('coach:foodAdmin.deleteRecipe')}
                            onConfirm={() => handleDeleteRecipe(recipe.id)}
                            okText={t('common:confirm.yes')}
                            cancelText={t('common:confirm.no')}
                            okButtonProps={{ danger: true }}
                          >
                            <Button type="primary" danger shape="circle" icon={<span className="material-symbols-outlined" style={{ fontSize: 18 }}>delete</span>} />
                          </Popconfirm>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ),
          },
        ]}
      />

      {/* Manual Add/Edit Food Modal */}
      <Modal
        title={editingFood ? t('coach:foodAdmin.modalEditFood') : t('coach:foodAdmin.modalAddFood')}
        open={isFoodModalVisible}
        onCancel={() => setIsFoodModalVisible(false)}
        onOk={handleSaveFood}
        okText={editingFood ? t('common:actions.save') : t('common:actions.submit')}
        okButtonProps={{ loading: createFoodMutation.isPending || updateFoodMutation.isPending }}
        width={500}
      >
        <Form form={foodForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="name" label={t('coach:foodAdmin.foodName')} rules={[{ required: true, message: t('coach:foodAdmin.enterName') }]}>
            <Input placeholder={t('coach:foodAdmin.foodNamePlaceholder')} />
          </Form.Item>
          <Form.Item name="category" label={t('coach:foodAdmin.table.category')} rules={[{ required: true, message: t('coach:foodAdmin.selectCategory') }]}>
            <Select placeholder={t('coach:foodAdmin.selectCategory')} options={FOOD_CATEGORIES.map((c) => ({ label: getFoodCategoryLabel(c, t), value: c }))} />
          </Form.Item>

          <Form.Item name="caloriesPer100g" label={t('coach:foodAdmin.cal100g')} rules={[{ required: true, message: t('coach:foodAdmin.enterCalories') }]}>
            <InputNumber min={0} placeholder="e.g. 160" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="proteinPer100g" label={t('coach:foodAdmin.prot100g')} rules={[{ required: true, message: t('coach:foodAdmin.enterProtein') }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 2.0" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="carbsPer100g" label={t('coach:foodAdmin.carb100g')} rules={[{ required: true, message: t('coach:foodAdmin.enterCarbs') }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 8.5" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="fatPer100g" label={t('coach:foodAdmin.fat100g')} rules={[{ required: true, message: t('coach:foodAdmin.enterFat') }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 14.7" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="fiberPer100g" label={t('coach:foodAdmin.fib100g')} rules={[{ required: true, message: t('coach:foodAdmin.enterFiber') }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 6.7" style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Bulk Import Modal */}
      <BulkImportModal
        visible={isBulkImportVisible}
        onCancel={() => setIsBulkImportVisible(false)}
      />

      {/* Recipe Wizard Modal */}
      <CreateRecipeModal
        open={isRecipeModalVisible}
        onClose={() => {
          setIsRecipeModalVisible(false);
          setEditingRecipe(null);
        }}
        recipeToEdit={editingRecipe}
        source="coach"
      />
    </div>
  );
};

export default FoodRecipeAdmin;
