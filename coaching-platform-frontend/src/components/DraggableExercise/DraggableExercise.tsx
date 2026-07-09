import React from 'react';
import { useDraggable } from '@dnd-kit/core';
import { Dropdown } from 'antd';
import './DraggableExercise.scss';

interface DraggableExerciseProps {
  id: string; // unique drag id
  exercise: {
    id: number;
    name: string;
    primaryMuscle: string;
  };
  days?: { dayNumber: number; dayLabel: string }[];
  onAddToDay?: (exercise: any, dayNumber: number) => void;
}

const DraggableExercise: React.FC<DraggableExerciseProps> = ({ id, exercise, days, onAddToDay }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: id,
    data: {
      exercise,
    },
  });

  const style = transform
    ? {
        transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 9999 : 'auto',
      }
    : undefined;

  const menuItems = days?.map((day) => ({
    key: String(day.dayNumber),
    label: day.dayLabel,
  }));

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`draggable-exercise ${isDragging ? 'draggable-exercise--dragging' : ''}`}
    >
      <div 
        className="draggable-exercise__drag-handle" 
        {...listeners} 
        {...attributes}
        style={{ cursor: 'grab' }}
      >
        <span className="material-symbols-outlined">drag_indicator</span>
      </div>
      <div className="draggable-exercise__content">
        <h4 className="draggable-exercise__name">{exercise.name}</h4>
        <span className="draggable-exercise__muscle-badge">{exercise.primaryMuscle}</span>
      </div>
      {onAddToDay && days && days.length > 0 && (
        <Dropdown
          menu={{
            items: menuItems,
            onClick: ({ key }) => onAddToDay(exercise, parseInt(key, 10)),
          }}
          trigger={['click']}
          placement="bottomRight"
        >
          <button
            className="draggable-exercise__add-btn"
            onClick={(e) => {
              e.stopPropagation();
              e.preventDefault();
            }}
            title="Add to Day"
            aria-label="Add to workout day"
          >
            <span className="material-symbols-outlined">add</span>
          </button>
        </Dropdown>
      )}
    </div>
  );
};

export default DraggableExercise;
