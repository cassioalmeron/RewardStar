import React, { useState } from 'react';
import { DragDropContext, Droppable, Draggable, DropResult } from '@hello-pangea/dnd';
import './Styles.css';

interface TestItem {
  id: string;
  content: string;
  position: number;
}

const Tests: React.FC = () => {
  const [tests, setTests] = useState<TestItem[]>([
    { id: '1', content: 'Unit Test: Activity Creation', position: 1 },
    { id: '2', content: 'Integration Test: API Communication', position: 2 },
    { id: '3', content: 'Performance Test: Data Loading', position: 3 },
    { id: '4', content: 'UI Test: Navigation Flow', position: 4 },
    { id: '5', content: 'Security Test: Authentication', position: 5 },
    { id: '6', content: 'Database Test: CRUD Operations', position: 6 },
    { id: '7', content: 'Mobile Test: Responsive Design', position: 7 },
    { id: '8', content: 'Accessibility Test: Screen Reader', position: 8 },
  ]);

  const handleDragEnd = (result: DropResult) => {
    if (!result.destination) return;

    const items = Array.from(tests);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    // Update positions after reordering
    const updatedItems = items.map((item, index) => ({
      ...item,
      position: index + 1
    }));

    setTests(updatedItems);
  };

  const showOrder = () => {
    const orderMessage = tests.map((test) => 
      `${test.position}. ${test.content}`
    ).join('\n');
    alert('Current Order:\n\n' + orderMessage);
  };

  return (
    <div className="tests-container">
      <h1>Tests</h1>
      <div className="tests-content">
        <p>Welcome to the Tests page!</p>
        <DragDropContext onDragEnd={handleDragEnd}>
          <Droppable droppableId="tests">
            {(provided) => (
              <table className="tests-table">
                <thead>
                  <tr>
                    <th>Test Description</th>
                  </tr>
                </thead>
                <tbody
                  {...provided.droppableProps}
                  ref={provided.innerRef}
                >
                  {tests.map((test, index) => (
                    <Draggable key={test.id} draggableId={test.id} index={index}>
                      {(provided, snapshot) => (
                        <tr
                          ref={provided.innerRef}
                          {...provided.draggableProps}
                          {...provided.dragHandleProps}
                          className={`test-row ${snapshot.isDragging ? 'dragging' : ''}`}
                        >
                          <td className="test-content">{test.content}</td>
                        </tr>
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </tbody>
              </table>
            )}
          </Droppable>
        </DragDropContext>
        <button onClick={showOrder} className="show-order-btn">
          Show Current Order
        </button>
      </div>
    </div>
  );
};

export default Tests; 