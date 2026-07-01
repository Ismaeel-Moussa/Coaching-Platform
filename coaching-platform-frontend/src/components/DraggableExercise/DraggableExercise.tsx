import React from 'react';
import { useDraggable } from '@dnd-kit/core';
import './DraggableExercise.scss';

interface DraggableExerciseProps {
  id: string; // unique drag id
  exercise: {
    id: number;
    name: string;
    primaryMuscle: string;
  };
}

const DraggableExercise: React.FC<DraggableExerciseProps> = ({ id, exercise }) => {
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

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      className={`draggable-exercise ${isDragging ? 'draggable-exercise--dragging' : ''}`}
    >
      <div className="draggable-exercise__drag-handle">
        <span className="material-symbols-outlined">drag_indicator</span>
      </div>
      <div className="draggable-exercise__content">
        <h4 className="draggable-exercise__name">{exercise.name}</h4>
        <span className="draggable-exercise__muscle-badge">{exercise.primaryMuscle}</span>
      </div>
    </div>
  );
};

export default DraggableExercise;
