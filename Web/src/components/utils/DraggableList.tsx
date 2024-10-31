import { createRef } from 'react';
import { useSortable } from 'use-sortablejs';
import './DraggableList.css';

export interface IDraggableListProps {
    items: Record<string, JSX.Element>;
    order: string[];
    onChange: (order: string[]) => void;
}

export function DraggableList({ items, order, onChange }: IDraggableListProps) {
    const { getRootProps, getItemProps } = useSortable<string>({
        setItems: (setAction) => (typeof setAction === 'function' ? onChange(setAction(order)) : onChange(order)),
        options: { animation: 150, ghostClass: 'sortable-ghost' },
    });

    const ref = createRef<HTMLDivElement>();

    return (
        <div {...getRootProps()} className="sortable">
            {order.map((key, i) => (
                <div key={key} {...getItemProps(key)}>
                    {items[key]}
                </div>
            ))}
        </div>
    );
}
