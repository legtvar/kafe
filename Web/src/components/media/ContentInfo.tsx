import { Box, Icon, IconButton, Tooltip } from '@chakra-ui/react';
import { t } from 'i18next';
import React, { useState } from 'react';
import { IoInformationCircleOutline } from 'react-icons/io5';
import { Artifact } from '../../data/Artifact';

interface ContentInfoProps {
  artifact: Artifact;
}

export function ContentInfo({ artifact }: ContentInfoProps) {
    const [showInfo, setShowInfo] = useState(false);

    const artifactInfo = t('generic.addedOn') + ": " + `${artifact.getAddedOn()?.toLocaleDateString()} ${artifact.getAddedOn()?.toLocaleTimeString()}`;

    return (
        <Tooltip label={artifactInfo} bg="gray.700" color="white">
          <IconButton
            aria-label="Info"
            icon={
              <IoInformationCircleOutline style={{ width: '100%', height: '100%' }} />
            }
            size="xs"
            variant="ghost"
          />
        </Tooltip>
      );
}
