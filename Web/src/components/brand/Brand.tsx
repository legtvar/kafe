import { CSSProperties } from 'react';
import { BrandBroken } from './src/BrandBroken';
import { BrandCracked } from './src/BrandCracked';
import { BrandDefault } from './src/BrandDefault';
import { BrandSimple } from './src/BrandSimple';
import { BrandStripe } from './src/BrandStripe';

interface IBrandProps {
    variant?: 'default' | 'broken' | 'cracked' | 'simple' | 'stripe';
    fill?: true;
    strokeWeight?: number;
    stroke?: string;
}

export function Brand({ variant, fill, strokeWeight, stroke }: IBrandProps) {
    const svgStyle: CSSProperties = {
        display: 'inline-block',
        fillRule: 'evenodd',
        clipRule: 'evenodd',
        strokeLinecap: 'round',
        strokeLinejoin: 'round',
        strokeMiterlimit: 1.5,
        height: '1em',
    };
    const outlineStyle: CSSProperties = {
        fill: 'none',
        stroke: stroke || 'currentColor',
        strokeWidth: strokeWeight || 15,
        strokeLinejoin: 'miter',
    };
    const filledStyle: CSSProperties = fill ? { ...outlineStyle, fill: 'rgb(206,171,147)' } : outlineStyle;

    switch (variant) {
        case 'broken':
            return (
                <BrandBroken svgStyle={svgStyle} filledStyle={filledStyle} outlineStyle={outlineStyle}></BrandBroken>
            );
        case 'cracked':
            return (
                <BrandCracked svgStyle={svgStyle} filledStyle={filledStyle} outlineStyle={outlineStyle}></BrandCracked>
            );
        case 'simple':
            return (
                <BrandSimple svgStyle={svgStyle} filledStyle={filledStyle} outlineStyle={outlineStyle}></BrandSimple>
            );
        case 'stripe':
            return (
                <BrandStripe svgStyle={svgStyle} filledStyle={filledStyle} outlineStyle={outlineStyle}></BrandStripe>
            );
        default:
            return (
                <BrandDefault svgStyle={svgStyle} filledStyle={filledStyle} outlineStyle={outlineStyle}></BrandDefault>
            );
    }
}
