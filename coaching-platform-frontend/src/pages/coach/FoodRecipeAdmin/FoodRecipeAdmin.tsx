import React, { useState } from 'react';
import { Tabs, Table, Button, Input, Select, Space, Popconfirm, Modal, Form, InputNumber, Empty, Radio, Tag } from 'antd';
import { useSearchFoods, useCreateFood, useUpdateFood, useDeleteFood } from '../../../hooks/useFoods/useFoods';
import { useGetRecipes, useDeleteRecipe } from '../../../hooks/useRecipes/useRecipes';
import BulkImportModal from '../../../components/BulkImportModal/BulkImportModal';
import CreateRecipeModal from '../../../components/CreateRecipeModal/CreateRecipeModal';
import RecipeCard from '../../../components/RecipeCard/RecipeCard';
import type { FoodDto, FoodCategory, FoodState, CreateFoodForm } from '../../../types/Food';
import type { RecipeDto, GetRecipesParams } from '../../../types/Recipe';
import { RecipeCategory } from '../../../types/Recipe';
import './FoodRecipeAdmin.scss';

const FOOD_CATEGORIES: FoodCategory[] = ['Protein', 'Carbs', 'Fat', 'Vegetable', 'Dairy', 'Fruit', 'Meat', 'Bakery', 'Cheese', 'Poultry', 'Nuts', 'Oils', 'Condiments', 'Juice', 'Snacks'];
const FOOD_STATES: { label: string; value: FoodState; color: string; icon: string }[] = [
  { label: 'Raw',    value: 'Raw',    color: 'green',  icon: '🥩' },
  { label: 'Cooked', value: 'Cooked', color: 'orange', icon: '🍳' },
  { label: 'Dry',    value: 'Dry',    color: 'gold',   icon: '🌾' },
];
const RECIPE_CATEGORIES = [
  { label: 'All Recipes', value: 'All' },
  { label: 'Muscle Building', value: RecipeCategory.MuscleBuilding.toString() },
  { label: 'Fat Loss', value: RecipeCategory.FatLoss.toString() },
  { label: 'Custom', value: RecipeCategory.Custom.toString() },
];

const FoodRecipeAdmin: React.FC = () => {
  const [activeTab, setActiveTab] = useState<string>('foods');

  // Foods state
  const [foodSearch, setFoodSearch] = useState<string>('');
  const [foodCategory, setFoodCategory] = useState<FoodCategory | undefined>(undefined);
  const [foodState, setFoodState] = useState<FoodState | undefined>(undefined);
  const [foodPage, setFoodPage] = useState<number>(1);
  const [isFoodModalVisible, setIsFoodModalVisible] = useState<boolean>(false);
  const [editingFood, setEditingFood] = useState<FoodDto | null>(null);
  const [isBulkImportVisible, setIsBulkImportVisible] = useState<boolean>(false);

  // Recipes state
  const [recipeCategory, setRecipeCategory] = useState<string>('All');
  const [isRecipeModalVisible, setIsRecipeModalVisible] = useState<boolean>(false);
  const [editingRecipe, setEditingRecipe] = useState<RecipeDto | null>(null);

  const [foodForm] = Form.useForm();

  // Queries & Mutations
  const { data: foodsData, isLoading: isFoodsLoading } = useSearchFoods({
    search: foodSearch || undefined,
    category: foodCategory,
    state: foodState,
    page: foodPage,
    pageSize: 15,
  });

  const { data: recipesData, isLoading: isRecipesLoading } = useGetRecipes({
    category: recipeCategory !== 'All' ? parseInt(recipeCategory, 10) : undefined,
  } as GetRecipesParams);

  const createFoodMutation = useCreateFood();
  const updateFoodMutation = useUpdateFood();
  const deleteFoodMutation = useDeleteFood();
  const deleteRecipeMutation = useDeleteRecipe();

  // Food CRUD handlers
  const handleAddFoodClick = () => {
    setEditingFood(null);
    foodForm.resetFields();
    foodForm.setFieldsValue({ state: 'Raw' });
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
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string) => <strong className="food-table__name">{text}</strong>,
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      render: (cat: string) => <span className="food-table__category font-data">{cat}</span>,
    },
    {
      title: 'State',
      dataIndex: 'state',
      key: 'state',
      render: (state: FoodState) => {
        const s = FOOD_STATES.find((x) => x.value === state);
        return (
          <Tag color={s?.color ?? 'default'} style={{ fontWeight: 600, letterSpacing: 0.3 }}>
            {s?.icon} {state ?? 'Raw'}
          </Tag>
        );
      },
    },
    {
      title: 'Calories / 100g',
      dataIndex: 'caloriesPer100g',
      key: 'caloriesPer100g',
      render: (val: number) => <span className="mono">{Math.round(val)} kcal</span>,
    },
    {
      title: 'Protein',
      dataIndex: 'proteinPer100g',
      key: 'proteinPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: 'Carbs',
      dataIndex: 'carbsPer100g',
      key: 'carbsPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: 'Fat',
      dataIndex: 'fatPer100g',
      key: 'fatPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: 'Fiber',
      dataIndex: 'fiberPer100g',
      key: 'fiberPer100g',
      render: (val: number) => <span className="mono">{val}g</span>,
    },
    {
      title: 'Actions',
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
            title="Delete food item?"
            description="Are you sure you want to delete this food?"
            onConfirm={() => handleDeleteFood(record.id)}
            okText="Yes"
            cancelText="No"
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
          <h1 className="food-recipe-admin__title">Nutrition &amp; Content Admin</h1>
          <p className="food-recipe-admin__subtitle">Manage master food databases and premium recipes</p>
        </div>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={(key) => setActiveTab(key)}
        className="food-recipe-admin__tabs"
        items={[
          {
            key: 'foods',
            label: 'Food Database',
            children: (
              <div className="food-recipe-admin__tab-content">
                <div className="food-recipe-admin__controls">
                  <div className="food-recipe-admin__filters">
                    <Input
                      placeholder="Search foods..."
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
                      placeholder="Category"
                      value={foodCategory}
                      onChange={(val) => {
                        setFoodCategory(val);
                        setFoodPage(1);
                      }}
                      allowClear
                      style={{ width: 160 }}
                      options={FOOD_CATEGORIES.map((cat) => ({ label: cat, value: cat }))}
                    />
                    <Select
                      placeholder="State"
                      value={foodState}
                      onChange={(val) => {
                        setFoodState(val);
                        setFoodPage(1);
                      }}
                      allowClear
                      style={{ width: 130 }}
                      options={FOOD_STATES.map((s) => ({
                        label: (
                          <span>
                            {s.icon} {s.label}
                          </span>
                        ),
                        value: s.value,
                      }))}
                    />
                  </div>
                  <Space>
                    <Button
                      type="default"
                      onClick={() => setIsBulkImportVisible(true)}
                      icon={<span className="material-symbols-outlined">upload_file</span>}
                      className="food-recipe-admin__btn"
                    >
                      Bulk Import
                    </Button>
                    <Button
                      type="primary"
                      onClick={handleAddFoodClick}
                      icon={<span className="material-symbols-outlined">add</span>}
                      className="food-recipe-admin__btn food-recipe-admin__btn--navy"
                    >
                      Add Food
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
                    pageSize: 15,
                    total: foodsData?.totalCount || 0,
                    onChange: (page) => setFoodPage(page),
                  }}
                  className="food-recipe-admin__table"
                />
              </div>
            ),
          },
          {
            key: 'recipes',
            label: 'Joker Recipes',
            children: (
              <div className="food-recipe-admin__tab-content">
                <div className="food-recipe-admin__controls">
                  <Tabs
                    activeKey={recipeCategory}
                    onChange={(key) => setRecipeCategory(key)}
                    items={RECIPE_CATEGORIES.map((cat) => ({
                      key: cat.value,
                      label: cat.label,
                    }))}
                    className="food-recipe-admin__recipe-tabs"
                  />
                  <Button
                    type="primary"
                    onClick={() => {
                      setEditingRecipe(null);
                      setIsRecipeModalVisible(true);
                    }}
                    icon={<span className="material-symbols-outlined">restaurant_menu</span>}
                    className="food-recipe-admin__btn food-recipe-admin__btn--navy"
                  >
                    Create Recipe
                  </Button>
                </div>

                {isRecipesLoading ? (
                  <div className="food-recipe-admin__recipe-grid">
                    {[1, 2, 3].map((i) => (
                      <div key={i} className="recipe-skeleton" style={{ height: 260, background: '#fff', borderRadius: 8, border: '1px solid var(--color-border)' }} />
                    ))}
                  </div>
                ) : recipesData?.items.length === 0 ? (
                  <Empty description="No recipes created yet." style={{ padding: '60px 0' }} />
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
                            title="Delete this recipe?"
                            onConfirm={() => handleDeleteRecipe(recipe.id)}
                            okText="Yes"
                            cancelText="No"
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
        title={editingFood ? 'Edit Food Item' : 'Add Food Item'}
        open={isFoodModalVisible}
        onCancel={() => setIsFoodModalVisible(false)}
        onOk={handleSaveFood}
        okText={editingFood ? 'Save' : 'Add'}
        okButtonProps={{ loading: createFoodMutation.isPending || updateFoodMutation.isPending }}
        width={500}
      >
        <Form form={foodForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="name" label="Food Name" rules={[{ required: true, message: 'Enter food name' }]}>
            <Input placeholder="e.g. Avocado (Fresh)" />
          </Form.Item>
          <Form.Item name="category" label="Category" rules={[{ required: true, message: 'Select category' }]}>
            <Select placeholder="Select category" options={FOOD_CATEGORIES.map((c) => ({ label: c, value: c }))} />
          </Form.Item>
          <Form.Item
            name="state"
            label="Food State"
            rules={[{ required: true, message: 'Select food state' }]}
            initialValue="Raw"
          >
            <Radio.Group buttonStyle="solid">
              {FOOD_STATES.map((s) => (
                <Radio.Button key={s.value} value={s.value}>
                  {s.icon} {s.label}
                </Radio.Button>
              ))}
            </Radio.Group>
          </Form.Item>
          <Form.Item name="caloriesPer100g" label="Calories per 100g" rules={[{ required: true, message: 'Enter calories' }]}>
            <InputNumber min={0} placeholder="e.g. 160" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="proteinPer100g" label="Protein (g) per 100g" rules={[{ required: true, message: 'Enter protein' }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 2.0" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="carbsPer100g" label="Carbs (g) per 100g" rules={[{ required: true, message: 'Enter carbs' }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 8.5" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="fatPer100g" label="Fat (g) per 100g" rules={[{ required: true, message: 'Enter fat' }]}>
            <InputNumber min={0} precision={1} placeholder="e.g. 14.7" style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="fiberPer100g" label="Fiber (g) per 100g" rules={[{ required: true, message: 'Enter fiber' }]}>
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
      />
    </div>
  );
};

export default FoodRecipeAdmin;
