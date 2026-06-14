/**
 * Case Creation Studio - Core Logic
 * Handles network visualization, entity management, and hierarchical processing.
 */


function caseStudio() {
    let _network = null;
    let _nodes = new vis.DataSet([]);
    let _edges = new vis.DataSet([]);

    return {
        get network() { return _network; },
        set network(val) { _network = val; },
        get nodes() { return _nodes; },
        get edges() { return _edges; },
        showSheet: false,
        isFinishingLink: false,
        activeTab: 'identity',
        activeLayout: 'hierarchical',
        isDraftModalOpen: false,
        activeDraftId: '',
        activeDraftName: 'Untitled Draft',
        savedDrafts: [],
        isDraftManagerOpen: false,
        isDraftSaving: false,
        lastDraftSaved: null,
        draftSummary: { nodeCount: 0, edgeCount: 0 },
        savedDraftPayload: null,
        draftTimer: null,
        isRestoring: false,
        editingDraftId: null,
        deletingDraftId: null,
        editDraftNameInput: '',
        formData: {
            id: '',
            label: '',
            name: '',
            type: 'I',
            isRoot: false,
            nationality: '',
            dob: '',
            gender: '',
            profession: '',
            residence: '',
            placeOfBirth: '',
            cif: '',
            employer: '',
            employerIndustry: '',
            employerSector: '',
            sowsofcountry: '',
            goldenVisa: 'No',
            selectedIdTypes: [],
            passportId: '',
            passportIssueDate: '',
            passportExpiryDate: '',
            emiratesIdNumber: '',
            emiratesIdIssueDate: '',
            emiratesIdExpiryDate: '',
            productType: '',
            productValue: '',
            deliveryChannel: '',
            paymentMode: '',
            counterParty: '',
            counterPartyName: '',
            productRefNo: '',
            relationship: '',
            share: 0,
            designation: '',
            threshold: 85,
            isScreened: false,
            attachments: [],
            screeningSources: ['1'],
            isOCRMode: false,
            isBulkMode: false,
            showScreeningDropdown: false,
            tradeLicence: '',
            tradeLicenseAuthority: '',
            registrationDate: '',
            expiryDate: '',
            entityTypeTxt: '',
            businessType: '',
            matchParameter: 85,
            shareholderType: '',
            flagType: ''
        },
        studioLists: window["studioData"] || {},
        bulkRows: [],
        selectedBulkRows: [],
        searchQuery: '',
        isSearching: false,
        searchResult: null,
        bulkFiles: [],
        ocrResults: null,
        isDuplicateModalOpen: false,
        isProcessActionModalOpen: false,
        isSuccessModalOpen: false,
        duplicateToProcess: null,
        duplicateResults: [],
        searchNodeId: null,
        isGroupModalOpen: false,
        openSections: ['linkage', 'vault'], // Default open sections

        // Form Lists (populated by backend)
        errors: [],
        isConnectMode: false,
        connectionSourceId: null,
        activeTool: 'pointer', // 'pointer', 'hand', 'connect'
        showAddNode: false,
        showLayoutMenu: false,
        showGroupPopover: false,
        nodeMenuPos: { x: 0, y: 0 },
        hoveredNodeId: null,
        hoverTimer: null,
        selectedNode: null,
        appliedSearchQuery: '',
        searchId: '',
        selectionRect: { startX: 0, startY: 0, x: 0, y: 0, w: 0, h: 0, active: false },
        selectedNodes: [],
        collapsedNodes: [],
        treeNodes: [],
        groups: [], // Store group definitions { id, nodeIds, label }
        hoveredGroupId: null, // Track group being hovered
        activeGroupId: null, // Currently-focused group in modal
        isLinking: false,
        isGroupModalOpen: false, // Control for Group Settings Modal
        selectedGroupMembers: [], // Nodes selected within the group modal
        linkSourceId: null,
        linkSourcePos: { x: 0, y: 0 },
        linkMousePos: { x: 0, y: 0 },
        toolbarPos: { x: null, y: null },
        isDraggingToolbar: false,
        tbStartOffset: { x: 0, y: 0 },
        selectedEdgeId: null,
        activeEdgeRelationship: 'individual_shareholder',
        activeEdgeToType: '',
        edgeMenuPos: { x: 0, y: 0 },
        fetchQuery: '',
        fetchResults: [],
        showFetchConfirm: false,
        fetchConfirmId: null,
        isReadOnly: false,
        fetchHistory: [],
        isHistoryMode: false,
        caseRefId: '',
        marqueeOffset: 0,

        isLoading: false,
        isSubmitting: false,

        validateForm() {
            this.errors = [];

            // Core Mandatory Fields for Root Entities
            if (this.selectedNode?.isRoot) {
                if (this.formData.type === 'C') {
                    if (!this.formData.entityTypeTxt) this.errors.push('entityTypeTxt');
                    if (!this.formData.businessType) this.errors.push('businessType');
                } else {
                    if (!this.formData.profession) this.errors.push('profession');
                    if (!this.formData.residence) this.errors.push('residence');
                }

                if (!this.formData.productType) this.errors.push('productType');
                if (!this.formData.deliveryChannel) this.errors.push('deliveryChannel');
                if (!this.formData.paymentMode) this.errors.push('paymentMode');
            }
            
            // Name Pattern Validation (Shared) - Mirrored from InputGuards.cs
            const namePattern = /^[\u00C0-\u017F\p{L}\p{M}0-9 _\.\-'’]{0,100}$/u;
            if (this.formData.name && !namePattern.test(this.formData.name)) {
                toastr.error(`"${this.formData.name}" contains invalid characters (Only letters, spaces, dots, hyphens and apostrophes allowed).`);
                this.errors.push('name');
            }
            
            if (!this.formData.name && !this.selectedNode?.label) this.errors.push('name');

            if (this.errors.length > 0) {
                toastr.warning('Please fill all mandatory fields marked with *');
                return false;
            }
            return true;
        },
        validateAllNodes() {
            const allNodes = this.nodes.get();
            const allEdges = this.edges.get();

            if (allNodes.length === 0) {
                toastr.warning('Workspace is empty. Add entities before creating a case.');
                return false;
            }

            // 1. Connection Validation: All non-root nodes must have a parent connection
            const isolatedNodes = allNodes.filter(node => {
                if (node.isRoot) return false;
                // Check if this node is a target in any edge (meaning it has a parent)
                return !allEdges.some(edge => edge.to === node.id);
            });

            if (isolatedNodes.length > 0) {
                const firstIsolated = isolatedNodes[0];
                this.centerNode(firstIsolated.id);
                toastr.error('Isolated related entities found. Please connect all related parties to a parent entity before proceeding.', 'Validation Failed');
                return false;
            }

            // 2. Data Validation: Fill mandatory fields
            for (const node of allNodes) {
                // If it's a fetched node, we assume it's already valid or doesn't need re-validation for creation
                if (node.isFetched) continue;

                // Temporarily set formData to this node's data to use validateForm
                const originalData = JSON.parse(JSON.stringify(this.formData));
                const originalSelected = this.selectedNode;

                // Map node data to formData structure
                Object.keys(this.formData).forEach(key => {
                    if (node[key] !== undefined) this.formData[key] = node[key];
                });

                // Fallback for name if it's only in label (Skip system-generated labels)
                if (!this.formData.name && node.label && !node.label.includes('Main Entity') && !node.label.includes('Corporate') && !node.label.includes('Individual') && !node.label.includes('Related Party') && !node.label.includes('Shareholder') && !node.label.includes('Main Party')) {
                    this.formData.name = node.label;
                }

                // Extra mapping for type since it might be in different fields
                this.formData.type = node.type || node.customerType || 'I';

                // Set selectedNode so validateForm() can check isRoot for root-mandatory fields
                this.selectedNode = node;
                const isValid = this.validateForm();

                // Restore original state
                this.formData = originalData;
                this.selectedNode = originalSelected;

                if (!isValid) {
                    // Open the invalid node for the user to fix
                    this.centerNode(node.id);
                    // Use a delay to ensure the sheet is open before toastr
                    setTimeout(() => {
                        toastr.error(`Mandatory fields missing for entity: ${node.name || node.label || 'Unnamed Entity'}`);
                    }, 300);
                    return false;
                }
            }
            return true;
        },

        mapNodeToDto(node) {
            // Create the PascalCase DTO expected by CaseStudioPayload.cs
            const dto = {
                Id: node.id ? node.id.toString() : '',
                StudioId: node.id ? node.id.toString() : '',
                CaseId: node.caseId || node.CaseId || '',
                Label: node.label || '',
                Type: node.type || 'I',
                // Fix: Don't use label as fallback for Name to avoid saving "Main Entity #..." strings
                Name: node.name || '', 
                IsRoot: !!node.isRoot,

                // Identity
                FirstName: node.firstName || node.FirstName || '',
                MiddleName: node.middleName || node.MiddleName || '',
                LastName: node.lastName || node.LastName || '',
                Nationality: node.nationality || node.Nationality || '',
                Dob: node.dob || node.DOB || '',
                Gender: node.gender || node.Gender || '',
                Cif: node.cif || node.cifNumber || node.CIFNumber || '',

                // IDs
                PassportId: node.passportId || '',
                PassportIssueDate: node.passportIssueDate || '',
                PassportExpiryDate: node.passportExpiryDate || '',
                EmiratesIdNumber: node.emiratesIdNumber || '',
                EmiratesIdIssueDate: node.emiratesIdIssueDate || '',
                EmiratesIdExpiryDate: node.emiratesIdExpiryDate || '',
                SelectedIdTypes: node.selectedIdTypes || [],
                TradeLicence: node.tradeLicence || '',
                TradeLicenseAuthority: node.tradeLicenseAuthority || '',

                // Business/Profile
                EntityTypeTxt: node.entityTypeTxt || '',
                BusinessType: node.businessType || '',
                RegistrationDate: node.registrationDate || '',
                Profession: node.profession || '',
                Employer: node.employer || '',
                SourceOfFunds: node.sourceOfFunds || '',
                EstimatedIncome: node.estimatedIncome ? parseFloat(node.estimatedIncome) : null,

                // Compliance
                Relationship: node.relationship || '',
                ShareholderType: node.shareholderType || '',
                FlagType: node.flagType || '',
                Share: node.share ? parseFloat(node.share) : 0,
                MatchThreshold: parseInt(node.matchParameter || node.matchParameter) || 85,
                MatchScore: parseInt(node.matchScore || node.CustomerScreenMatchScore) || 0,
                IsScreened: !!node.isScreened,
                IsFetched: !!node.isFetched,
                IsMainEntity: !!node.isMainEntity,
                IsDuplicate: !!node.isDuplicate,
                IsJointParty: !!node.isJointParty,
                RiskCategory: node.riskCategory || '',
                ScreeningSources: node.screeningSources || (node.screeningOption ? [node.screeningOption] : ['1']),

                // Product
                ProductName: node.productName || node.productType || '',
                ProductValue: node.productValue ? parseFloat(node.productValue) : 0,
                DeliveryChannel: node.deliveryChannel || '',
                Modeofpayment: node.modeofpayment || node.paymentMode || '',
                Residence: node.residence || '',
                PlaceOfBirth: node.placeOfBirth || '',
                EmployerIndustry: node.employerIndustry || '',
                EmployerSector: node.employerSector || '',
                GoldenVisa: node.goldenVisa || '',
                CounterParty: node.counterParty || '',
                CounterPartyName: node.counterPartyName || '',
                ProductRefNo: node.productRefNo || '',
                Designation: node.designation || '',
                Sowsofcountry: node.sowsofcountry || '',
                GroupId: node.groupId ? (Array.isArray(node.groupId) ? node.groupId.join(',') : node.groupId) : '',
                GroupRisk: node.groupRisk ? (Array.isArray(node.groupRisk) ? node.groupRisk.join(',') : node.groupRisk) : '',
                GroupEntityof: (node.groupId && this.groups) ? (Array.isArray(node.groupId) ? node.groupId : [node.groupId]).map(gid => this.groups.find(g => g.id === gid)?.label).filter(l => l).join(', ') : ''
            };

            // Fix Id mapping for backend CustomerCaseDTO
            // StudioId preserves the canvas ID (e.g. S_...) for node tracking and edge resolution.
            // Id is strictly numeric for database identifiers (0 for new, numeric databaseId for existing).
            dto.Id = '0';
            if (node.databaseId) {
                const numericId = node.databaseId.toString().match(/\d+/);
                if (numericId) {
                    dto.Id = numericId[0];
                }
            }

            // Name Splitting fallback if empty
            if (dto.Name && (!dto.FirstName || dto.FirstName === dto.Name)) {
                const parts = dto.Name.trim().split(/\s+/);
                dto.FirstName = parts[0];
                dto.LastName = parts.length > 1 ? parts.slice(1).join(' ') : '';
            }

            return dto;
        },

        handleFileUpload(e) {
            const files = Array.from(e.target.files);
            files.forEach(f => {
                this.formData.attachments.push({
                    name: f.name,
                    size: f.size,
                    type: f.type,
                    date: new Date().toLocaleDateString()
                });
            });
            this.$nextTick(() => { if (window.lucide) lucide.createIcons(); });
        },
        formatSize(bytes) {
            if (!bytes) return '0 Bytes';
            if (typeof bytes === 'string' && bytes.includes(' ')) return bytes; // Already formatted
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        },
        removeAttachment(index) {
            this.formData.attachments.splice(index, 1);
        },
        viewFile(file) {
            const path = file.documentFullPath || file.DocumentFullPath || file.path;
            if (!path) {
                toastr.warning('File path not available for viewing.');
                return;
            }
            // If it's a relative path, ensure it starts with /
            const url = path.startsWith('http') ? path : (path.startsWith('/') ? path : '/' + path);
            window.open(url, '_blank');
        },
        downloadFile(file) {
            const path = file.documentFullPath || file.DocumentFullPath || file.path;
            const name = file.documentFileName || file.DocumentFileName || file.name || file.Name || 'DownloadedFile';
            if (!path) {
                toastr.warning('File path not available for download.');
                return;
            }
            window.location.href = `/Case/DownloadCaseDocument?filePath=${encodeURIComponent(path)}&fileName=${encodeURIComponent(name)}`;
        },

        triggerAutosave() {
            if (this.isRestoring || this.treeNodes.length === 0) return;
            clearTimeout(this.draftTimer);
            this.isDraftSaving = true;
            this.draftTimer = setTimeout(() => {
                this.saveDraft();
            }, 1500);
        },

        async saveDraft(isNew = false, customName = null) {
            if (this.isRestoring || this.treeNodes.length === 0) {
                this.isDraftSaving = false;
                return;
            }
            if (isNew) {
                this.activeDraftId = '';
                this.activeDraftName = customName || 'Draft - ' + new Date().toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
            } else if (!this.activeDraftId && (this.activeDraftName === 'Untitled Draft' || !this.activeDraftName)) {
                this.activeDraftName = customName || 'Draft - ' + new Date().toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
            }
            try {
                const nodes = Alpine.raw(this.nodes).get();
                const edges = Alpine.raw(this.edges).get();
                const mappedNodes = nodes.map(n => this.mapNodeToDto(n));

                if (!mappedNodes || mappedNodes.length === 0) {
                    this.isDraftSaving = false;
                    return;
                }
                const response = await fetch('/case/studio/draft', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        DraftId: this.activeDraftId,
                        DraftName: this.activeDraftName,
                        Nodes: mappedNodes,
                        Edges: edges.map(e => ({ 
                            From: e.from, 
                            To: e.to, 
                            Relationship: e.relationship || '', 
                            Share: e.share || null, 
                            Designation: e.designation || '', 
                            Remarks: e.remarks || '', 
                            Label: e.label || '' 
                        }))
                    })
                });
                const result = await response.json();
                if (result.success && result.draftId) {
                    this.activeDraftId = result.draftId;
                    const now = new Date();
                    this.lastDraftSaved = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    this.loadDraftSummaries();
                }
            } catch (err) {
                console.warn('Autosave draft failed:', err);
            } finally {
                this.isDraftSaving = false;
            }
        },

        async loadDraftSummaries() {
            try {
                const response = await fetch('/case/studio/drafts');
                const result = await response.json();
                if (result.success && result.drafts) {
                    this.savedDrafts = result.drafts;
                    if (this.savedDrafts.length > 0 && !this.activeDraftId && this.nodes.length === 0) {
                        this.isDraftManagerOpen = true;
                    }
                }
            } catch (err) {
                console.warn('Load draft summaries failed:', err);
            }
        },

        async checkExistingDraft() {
            await this.loadDraftSummaries();
        },

        async restoreSpecificDraft(draftId) {
            this.isRestoring = true;
            this.isDraftManagerOpen = false;
            try {
                const response = await fetch(`/case/studio/draft/${draftId}`);
                const result = await response.json();
                if (result.success && result.draft && result.draft.nodes) {
                    this.savedDraftPayload = result.draft;
                    this.activeDraftId = result.draft.draftId || draftId;
                    this.activeDraftName = result.draft.draftName || 'Restored Draft';

                    // Clear existing canvas
                    this.nodes.clear();
                    this.edges.clear();
                    this.groups = [];

                    const nodesRaw = Alpine.raw(this.nodes);
                    const edgesRaw = Alpine.raw(this.edges);

                    const viewCenter = this.network ? this.network.getViewPosition() : { x: 0, y: 0 };

                    // 1. Add all nodes
                    this.savedDraftPayload.nodes.forEach((nodeDto, index) => {
                        const nodeId = (nodeDto.studioId || nodeDto.StudioId || nodeDto.id || nodeDto.Id || `TEMP_${index}`).toString();
                        if (!nodesRaw.get(nodeId)) {
                            const type = this.normalizeType(nodeDto.type || nodeDto.Type || 'I');
                            const isRootNode = !!(nodeDto.isRoot || nodeDto.IsRoot);
                            const caseId = nodeDto.caseId || nodeDto.CaseId || nodeDto.cif || nodeDto.Cif || nodeDto.studioId || nodeDto.StudioId || '';
                            const gidRaw = nodeDto.groupId || nodeDto.GroupId || '';
                            const groupIds = gidRaw ? gidRaw.split(',').map(g => g.trim()).filter(Boolean) : [];

                            nodesRaw.add({
                                ...nodeDto,
                                id: nodeId,
                                caseId: caseId,
                                databaseId: nodeDto.id || nodeDto.Id || 0,
                                cif: nodeDto.cif || nodeDto.Cif || '',
                                label: nodeDto.label || nodeDto.Label || nodeDto.name || nodeDto.Name || nodeId,
                                name: nodeDto.name || nodeDto.Name || nodeDto.label || nodeDto.Label || nodeId,
                                attachments: nodeDto.attachments || [],
                                x: viewCenter.x + (index * 50),
                                y: viewCenter.y + (index * 50),
                                image: this.getSvg(type, isRootNode, !!(nodeDto.isScreened || nodeDto.IsScreened), !!(nodeDto.isDuplicate || nodeDto.IsDuplicate), caseId, undefined, !!(nodeDto.isMainEntity || nodeDto.IsMainEntity || isRootNode)),
                                type: type,
                                isRoot: isRootNode,
                                isScreened: !!(nodeDto.isScreened || nodeDto.IsScreened),
                                isFetched: !!(nodeDto.isFetched || nodeDto.IsFetched),
                                isMainEntity: !!(nodeDto.isMainEntity || nodeDto.IsMainEntity || isRootNode),
                                isDuplicate: !!(nodeDto.isDuplicate || nodeDto.IsDuplicate),
                                groupId: groupIds,
                                groupRisk: nodeDto.groupRisk || nodeDto.GroupRisk || 'Unclassified',
                                level: (this.activeLayout === 'hierarchical') ? (nodeDto.level !== undefined ? nodeDto.level : 0) : null,
                                shareholderType: nodeDto.shareholderType || nodeDto.ShareholderType || type
                            });
                        }
                    });

                    // 2. Sync Group Definitions from Nodes
                    nodesRaw.get().forEach(n => {
                        const gids = Array.isArray(n.groupId) ? n.groupId : (n.groupId ? [n.groupId] : []);
                        gids.forEach(gid => {
                            if (!gid) return;
                            let group = this.groups.find(g => g.id === gid);
                            if (!group) {
                                const groupIndex = this.groups.length + 1;
                                group = {
                                    id: gid,
                                    nodeIds: [],
                                    label: `Group G${groupIndex}`,
                                    risk: n.groupRisk || 'Unclassified'
                                };
                                this.groups.push(group);
                            }
                            if (!group.nodeIds.includes(n.id)) {
                                group.nodeIds.push(n.id);
                            }
                        });
                    });

                    // 3. Add edges
                    if (this.savedDraftPayload.edges) {
                        this.savedDraftPayload.edges.forEach(edgeDto => {
                            const from = (edgeDto.from || edgeDto.From)?.toString();
                            const to = (edgeDto.to || edgeDto.To)?.toString();
                            if (from && to && nodesRaw.get(from) && nodesRaw.get(to)) {
                                const edgeId = `edge-${from}-${to}`;
                                if (!edgesRaw.get(edgeId)) {
                                    const toNode = nodesRaw.get(to);
                                    const rel = edgeDto.relationship || edgeDto.Relationship || (toNode ? toNode.relationship : '') || (toNode?.type === 'C' ? 'corporate_shareholder' : 'individual_shareholder');
                                    edgesRaw.add({
                                        id: edgeId,
                                        from: from,
                                        to: to,
                                        relationship: rel,
                                        share: edgeDto.share || edgeDto.Share || null,
                                        designation: edgeDto.designation || edgeDto.Designation || '',
                                        remarks: edgeDto.remarks || edgeDto.Remarks || '',
                                        label: edgeDto.label || edgeDto.Label || (rel ? this.getRelationshipShortform(rel) : '')
                                    });
                                }
                            }
                        });
                    }

                    this.autoLayout('hierarchical', true);
                    toastr.success(`Draft "${this.activeDraftName}" restored successfully.`, 'Success');
                } else {
                    toastr.error('Failed to load draft payload.');
                }
            } catch (err) {
                console.error('Failed to restore draft:', err);
                toastr.error('Failed to restore draft.');
            } finally {
                this.isRestoring = false;
            }
        },

        async deleteSpecificDraft(draftId) {
            try {
                const response = await fetch(`/case/studio/draft/${draftId}`, { method: 'DELETE' });
                const result = await response.json();
                if (result.success) {
                    toastr.info('Draft deleted.');
                    if (this.activeDraftId === draftId) {
                        this.activeDraftId = '';
                        this.activeDraftName = 'Untitled Draft';
                        this.nodes.clear();
                        this.edges.clear();
                        this.groups = [];
                    }
                    this.loadDraftSummaries();
                }
            } catch (err) {
                console.warn('Delete draft failed:', err);
            }
        },

        startInlineEdit(draft) {
            this.deletingDraftId = null;
            this.editingDraftId = draft.draftId;
            this.editDraftNameInput = draft.draftName;
        },

        async confirmInlineRename(draftId) {
            if (!this.editDraftNameInput || !this.editDraftNameInput.trim()) {
                toastr.warning('Draft name cannot be empty.');
                return;
            }
            const newName = this.editDraftNameInput.trim();
            try {
                const response = await fetch(`/case/studio/draft/${draftId}/rename`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ DraftName: newName })
                });
                const result = await response.json();
                if (result.success) {
                    toastr.success('Draft renamed successfully.');
                    if (this.activeDraftId === draftId) {
                        this.activeDraftName = newName;
                    }
                    this.editingDraftId = null;
                    this.loadDraftSummaries();
                } else {
                    toastr.error('Failed to rename draft.');
                }
            } catch (err) {
                console.error('Rename draft failed:', err);
                toastr.error('Failed to rename draft.');
            }
        },

        startInlineDelete(draft) {
            this.editingDraftId = null;
            this.deletingDraftId = draft.draftId;
        },

        async confirmInlineDelete(draftId) {
            if (!draftId) return;
            try {
                const response = await fetch(`/case/studio/draft/${draftId}`, { method: 'DELETE' });
                const result = await response.json();
                if (result.success) {
                    toastr.info('Draft deleted.');
                    if (this.activeDraftId === draftId) {
                        this.activeDraftId = '';
                        this.activeDraftName = 'Untitled Draft';
                        this.nodes.clear();
                        this.edges.clear();
                        this.groups = [];
                    }
                    this.deletingDraftId = null;
                    this.loadDraftSummaries();
                } else {
                    toastr.error('Failed to delete draft.');
                }
            } catch (err) {
                console.error('Delete draft failed:', err);
                toastr.error('Failed to delete draft.');
            }
        },

        createNewDraft(customName = null) {
            this.nodes.clear();
            this.edges.clear();
            this.groups = [];
            this.activeDraftId = '';
            this.activeDraftName = customName || 'Draft - ' + new Date().toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
            this.isDraftManagerOpen = false;
            toastr.info('Started new draft session.');
        },

        async restoreDraft() {
            if (this.savedDrafts.length > 0) {
                await this.restoreSpecificDraft(this.savedDrafts[0].draftId);
            }
        },

        async discardDraft() {
            if (this.activeDraftId) {
                await this.deleteSpecificDraft(this.activeDraftId);
            }
        },

        init() {
            this.updateTree();
            this.nodes.on('*', () => { this.updateTree(); this.triggerAutosave(); });
            this.edges.on('*', () => { this.updateTree(); this.triggerAutosave(); });
            this.checkExistingDraft();

            this.$watch('appliedSearchQuery', (val) => {
                if (!val) return;
                const match = this.treeNodes.find(n => n.isMatch);
                if (match) this.centerNode(match.id);
            });

            this.$watch('fetchResults', () => {
                this.$nextTick(() => { if (window.lucide) lucide.createIcons(); });
            });

            this.$watch('showFetchConfirm', (val) => {
                if (val) this.$nextTick(() => { if (window.lucide) lucide.createIcons(); });
            });

            this.$watch('showSheet', (val) => {
                if (val) this.$nextTick(() => { if (window.lucide) lucide.createIcons(); });
            });

            this.$watch('activeTab', () => {
                this.$nextTick(() => { if (window.lucide) lucide.createIcons(); });
            });

            this.$watch('formData', () => {
                if (this.selectedNode && !this.isReadOnly) {
                    this.updateActiveNodeLabel();
                }
            }, { deep: true });

            this.loadHistory();

            const container = document.getElementById('studio-canvas');
            const data = { nodes: this.nodes, edges: this.edges };
            const options = {
                nodes: {
                    shape: 'image',
                    size: 45,
                    font: { face: 'Inter', size: 12, color: '#1E293B', strokeWidth: 4, strokeColor: '#FFFFFF' },
                    borderWidth: 0,
                    shadow: { enabled: true, color: 'rgba(0,0,0,0.05)', size: 10, x: 0, y: 5 }
                },
                edges: {
                    width: 2,
                    color: { color: '#CBD5E1', highlight: '#2563EB', hover: '#94A3B8' },
                    smooth: { type: 'continuous', roundness: 0.5 },
                    arrows: { from: { enabled: true, scaleFactor: 0.5 } },
                    selectionWidth: 3,
                    font: { align: 'middle', size: 11, face: 'Inter', strokeWidth: 4, strokeColor: '#ffffff', color: '#64748B', weight: 'bold' }
                },
                interaction: {
                    hover: true,
                    multiselect: true,
                    dragView: true,
                    zoomView: true,
                    dragNodes: true,
                    navigationButtons: false,
                    keyboard: true
                },
                physics: { enabled: false }
            };

            this.network = new vis.Network(container, data, options);

            const network = Alpine.raw(this.network);

            // Auto-disable physics after stabilization to prevent 'drag-drift'
            network.on('stabilized', () => {
                network.setOptions({ physics: { enabled: false } });
                console.log('Canvas Stabilized: Physics disabled');
            });

            // Ensure canvas size is correctly detected and center the view
            setTimeout(() => {
                network.redraw();
                network.fit();
            }, 200);

            // Interaction Handlers
            network.on('click', (params) => {
                if (this.activeTool === 'hand') return; // No interaction in hand mode

                const nodes = Alpine.raw(this.nodes);

                if (this.activeTool === 'connect' && params.nodes.length === 1) {
                    if (this.linkSourceId && this.linkSourceId !== params.nodes[0]) {
                        // Check for cycles
                        if (this.canReach(params.nodes[0], this.linkSourceId)) {
                            toastr.error('Circular relationships are not allowed.');
                        } else {
                            this.createEdge(this.linkSourceId, params.nodes[0]);
                            toastr.success('Connection established.');
                        }
                        this.cancelLink();
                    } else {
                        this.startConnection(params.nodes[0]);
                    }
                } else if (params.nodes.length === 1) {
                    const isMulti = params.event.srcEvent.shiftKey || params.event.srcEvent.ctrlKey || params.event.srcEvent.metaKey;
                    if (isMulti) {
                        this.selectedNodes = network.getSelectedNodes().map(id => id.toString());
                        this.closeSheet();
                    } else {
                        this.openNode(nodes.get(params.nodes[0]));
                        this.selectedNodes = [params.nodes[0].toString()];
                    }
                    this.updateBoundingBox();
                } else if (params.edges.length === 1 && params.nodes.length === 0) {
                    this.selectedEdgeId = params.edges[0];
                    const edgeObj = Alpine.raw(this.edges).get(this.selectedEdgeId);
                    // Resolve the type of the destination node so dropdown options are filtered correctly
                    const toNodeId = edgeObj ? edgeObj.to : null;
                    const toNodeObj = toNodeId ? Alpine.raw(this.nodes).get(toNodeId) : null;
                    this.activeEdgeToType = toNodeObj ? (toNodeObj.type || 'I') : 'I';
                    // Default relationship based on destination node type, not a hard-coded ISH
                    const typeBasedDefault = this.activeEdgeToType === 'C' ? 'corporate_shareholder' : 'individual_shareholder';
                    this.activeEdgeRelationship = edgeObj ? (edgeObj.relationship || typeBasedDefault) : typeBasedDefault;
                    const canvasPos = params.pointer.canvas;
                    const domPos = network.canvasToDOM(canvasPos);
                    const canvas = document.getElementById('studio-canvas');
                    this.edgeMenuPos = { x: domPos.x + canvas.offsetLeft, y: domPos.y + canvas.offsetTop - 100 };
                    this.closeSheet();
                } else {
                    this.closeSheet();
                    this.selectedNodes = network.getSelectedNodes().map(id => id.toString());
                    this.selectedEdgeId = null;
                    this.updateBoundingBox();
                }
            });

            network.on('afterDrawing', (ctx) => {
                this.renderGroups(ctx);
                this.renderSelectionMarquee(ctx);
                this.updateBoundingBox();
            });

            const animateMarquee = () => {
                if (this.selectedNodes.length > 1) {
                    this.marqueeOffset = (this.marqueeOffset + 0.4) % 20;
                    if (this.network) this.network.redraw();
                }
                requestAnimationFrame(animateMarquee);
            };
            requestAnimationFrame(animateMarquee);

            network.on('oncontext', (params) => {
                if (this.activeTool === 'hand') return;
                params.event.preventDefault();
                const nodeId = network.getNodeAt(params.pointer.DOM);
                if (nodeId) {
                    this.hoveredNodeId = nodeId;
                    const domPos = params.pointer.DOM;
                    const canvas = document.getElementById('studio-canvas');
                    this.nodeMenuPos = { x: domPos.x + canvas.offsetLeft, y: domPos.y + canvas.offsetTop - 100 };
                }
            });

            network.on('hoverNode', (params) => {
                if (this.activeTool === 'hand' || this.isLinking) return;
                clearTimeout(this.hoverTimer);
                this.hoveredNodeId = params.node;
                const pos = network.getPositions([params.node])[params.node];
                const domPos = network.canvasToDOM(pos);
                const canvas = document.getElementById('studio-canvas');
                this.nodeMenuPos = { x: domPos.x + canvas.offsetLeft, y: domPos.y + canvas.offsetTop - 100 };
            });

            network.on('blurNode', () => {
                this.hoverTimer = setTimeout(() => { this.hoveredNodeId = null; }, 800);
            });

            this.network.on('dragStart', () => {
                this.hoveredNodeId = null;
                document.getElementById('selection-box').style.display = 'none';
            });

            this.network.on('dragging', () => {
                this.updateBoundingBox();
                this.updateGroupOverlays();
            });

            this.network.on('dragEnd', (params) => {
                if (params.nodes.length > 0) {
                    const positions = this.network.getPositions(params.nodes);
                    params.nodes.forEach(id => {
                        // Sync position to DataSet to prevent snap-back
                        this.nodes.update({ id, x: positions[id].x, y: positions[id].y });
                    });
                }
                this.updateBoundingBox();
                this.updateGroupOverlays();
            });

            this.network.on('zoom', () => {
                this.updateBoundingBox();
                this.updateGroupOverlays();
            });

            this.network.on('dragView', () => {
                this.updateBoundingBox();
                this.updateGroupOverlays();
            });

            // Marquee Selection Logic
            const canvas = document.getElementById('studio-canvas');
            canvas.addEventListener('mousedown', (e) => {
                const isMulti = e.shiftKey || e.ctrlKey || e.metaKey;
                if (this.activeTool !== 'pointer' || e.button !== 0 || this.network.getNodeAt({ x: e.offsetX, y: e.offsetY })) return;

                if (isMulti) {
                    e.preventDefault();
                    this.selectionRect.active = true;
                    this.selectionRect.x = e.clientX;
                    this.selectionRect.y = e.clientY;

                    const rectEl = document.getElementById('marquee-rect');
                    rectEl.style.display = 'block';
                    rectEl.style.left = e.clientX + 'px';
                    rectEl.style.top = e.clientY + 'px';
                    rectEl.style.width = '0px';
                    rectEl.style.height = '0px';

                    // Disable vis.js internal drag during marquee
                    this.network.setOptions({ interaction: { dragView: false } });
                }
            });

            document.addEventListener('mousemove', (e) => {
                if (this.isLinking) {
                    const rect = canvas.getBoundingClientRect();
                    this.linkMousePos = { x: e.clientX - rect.left, y: e.clientY - rect.top };
                }

                // Group Hover Detection
                if (this.activeTool === 'pointer' && !this.showSheet) {
                    if (e.target.closest('.group-label-overlay')) return;

                    const rect = canvas.getBoundingClientRect();
                    const network = Alpine.raw(this.network);
                    if (network) {
                        const canvasPos = network.DOMtoCanvas({
                            x: e.clientX - rect.left,
                            y: e.clientY - rect.top
                        });

                        // Hover detection optimization:
                        // Only show group hovertip if hovering over a group member or inside boundary
                        const hoveredNodeId = network.getNodeAt({ x: e.clientX - rect.left, y: e.clientY - rect.top });

                        let foundGroups = [];
                        if (hoveredNodeId) {
                            const node = network.body.nodes[hoveredNodeId];
                            foundGroups = Array.isArray(node.options.groupId) ? node.options.groupId : (node.options.groupId ? [node.options.groupId] : []);
                        }

                        if (foundGroups.length === 0) {
                            for (const group of this.groups) {
                                const box = this.getGroupBox(group.nodeIds);
                                if (box &&
                                    canvasPos.x >= box.minX && canvasPos.x <= box.maxX &&
                                    canvasPos.y >= box.minY && canvasPos.y <= box.maxY) {
                                    foundGroups = [group.id];
                                    break;
                                }
                            }
                        }

                        // Use a composite ID for multiple groups to trigger the consolidated toolbar
                        const compositeId = foundGroups.length > 0 ? foundGroups.join(',') : null;

                        if (this.hoveredGroupId !== compositeId) {
                            this.hoveredGroupId = compositeId;
                            if (compositeId) this.updateGroupOverlays();
                        }
                    }
                }

                if (this.selectedNodes.length > 1) {
                    this.updateSelectionMenu();
                }

                if (!this.selectionRect.active) return;

                const currentX = e.clientX;
                const currentY = e.clientY;

                const x = Math.min(currentX, this.selectionRect.x);
                const y = Math.min(currentY, this.selectionRect.y);
                const w = Math.abs(currentX - this.selectionRect.x);
                const h = Math.abs(currentY - this.selectionRect.y);

                const rectEl = document.getElementById('marquee-rect');
                rectEl.style.left = (x) + 'px';
                rectEl.style.top = (y) + 'px';
                rectEl.style.width = w + 'px';
                rectEl.style.height = h + 'px';

                // Real-time selection logic
                const canvasRect = canvas.getBoundingClientRect();
                const p1 = this.network.DOMtoCanvas({ x: x - canvasRect.left, y: y - canvasRect.top });
                const p2 = this.network.DOMtoCanvas({ x: x + w - canvasRect.left, y: y + h - canvasRect.top });

                const allPos = this.network.getPositions();
                const nodesInRect = Object.keys(allPos).filter(id => {
                    const pos = allPos[id];
                    const minX = Math.min(p1.x, p2.x) - 20;
                    const maxX = Math.max(p1.x, p2.x) + 20;
                    const minY = Math.min(p1.y, p2.y) - 20;
                    const maxY = Math.max(p1.y, p2.y) + 20;
                    return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
                });

                const network = Alpine.raw(this.network);
                network.setSelection({ nodes: nodesInRect }, { highlightEdges: false });
                this.selectedNodes = nodesInRect.map(id => id.toString());
                this.updateBoundingBox();
            });

            document.addEventListener('mouseup', () => {
                if (!this.selectionRect.active) return;
                const rectEl = document.getElementById('marquee-rect');
                this.selectionRect.active = false;
                rectEl.style.display = 'none';

                // Restore interaction
                this.setTool(this.activeTool);
            });

            this.autoLayout('hierarchical', true);

            window.addEventListener('keydown', (e) => {
                if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

                if (e.key === 'Escape' && this.isLinking) this.cancelLink();
                if (e.key === 'Delete' || e.key === 'Backspace') {
                    const network = Alpine.raw(this.network);
                    const selectedNodes = network.getSelectedNodes();
                    if (selectedNodes.length > 0) {
                        this.deleteNodes(selectedNodes);
                    }
                }
            });
        },
        getGroupBox(nodeIds) {
            const network = Alpine.raw(this.network);
            if (!network || !nodeIds || nodeIds.length === 0) return null;

            const positions = network.getPositions(nodeIds);
            let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;

            nodeIds.forEach(id => {
                const pos = positions[id];
                if (pos) {
                    minX = Math.min(minX, pos.x);
                    minY = Math.min(minY, pos.y);
                    maxX = Math.max(maxX, pos.x);
                    maxY = Math.max(maxY, pos.y);
                }
            });

            if (minX === Infinity) return null;

            const padding = 80;
            const bottomPadding = 130;
            return {
                minX: minX - padding,
                minY: minY - padding,
                maxX: maxX + padding,
                maxY: maxY + bottomPadding,
                width: (maxX - minX) + (padding * 2),
                height: (maxY - minY) + padding + bottomPadding,
                minCanvasX: minX - padding,
                minCanvasY: minY - padding
            };
        },
        canReach(fromId, toId) {
            const edges = this.edges.get();
            const visited = new Set();
            const queue = [fromId.toString()];

            while (queue.length > 0) {
                const current = queue.shift();
                if (current === toId.toString()) return true;
                if (visited.has(current)) continue;
                visited.add(current);

                const nextNodes = edges.filter(e => e.from.toString() === current).map(e => e.to.toString());
                queue.push(...nextNodes);
            }
            return false;
        },

        updateTree() {
            this.treeNodes = this.getTreeNodes();
            this.$nextTick(() => {
                if (window.lucide) lucide.createIcons();
            });
        },
        getIconForType(node) {
            if (!node) return 'user';
            if (node.type === 'C' || node.customerType === 'C') return node.isRoot ? 'building-2' : 'building';
            return node.isRoot ? 'user' : 'user-plus';
        },
        formatDateForInput(date) {
            if (!date || typeof date !== 'string' || date.startsWith('0001-01-01')) return '';
            return date.split('T')[0].split(' ')[0]; // Handle both ISO and space-separated formats
        },
        startToolbarDrag(e) {
            this.isDraggingToolbar = true;
            const toolbar = e.currentTarget.closest('.floating-toolbar');
            this.tbStartOffset = { x: e.clientX - toolbar.offsetLeft, y: e.clientY - toolbar.offsetTop };
            toolbar.style.transition = 'none';
        },
        handleToolbarDrag(e) {
            if (!this.isDraggingToolbar) return;
            this.toolbarPos = { x: e.clientX - this.tbStartOffset.x, y: e.clientY - this.tbStartOffset.y };
        },
        stopToolbarDrag() {
            this.isDraggingToolbar = false;
            const toolbar = document.querySelector('.floating-toolbar');
            if (toolbar) toolbar.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        },
        updateActiveNodeLabel() {
            if (!this.selectedNode || this.isReadOnly) return;
            const shortId = this.selectedNode.shortId || this.selectedNode.id.toString().slice(-4);
            const fallback = (this.selectedNode.isRoot ? 'Main Entity' : 'Related Party') + ' #' + shortId;
            const label = this.formData.name || fallback;
            const nodes = Alpine.raw(this.nodes);
            
            // Sync EVERYTHING back to the dataset immediately to ensure reactivity across UI
            nodes.update({ 
                ...this.formData,
                id: this.selectedNode.id, 
                label: label,
                name: this.formData.name
            });
            
            // Keep local reference updated for header
            this.selectedNode.label = label;
            this.selectedNode.name = this.formData.name;
        },
        updateBoundingBox() {
            setTimeout(() => {
                const boxEl = document.getElementById('selection-box');
                const canvas = document.getElementById('studio-canvas');
                if (!canvas || !boxEl) return;

                const network = Alpine.raw(this.network);
                if (!network) return;

                const currentSelection = network.getSelection().nodes;
                if (currentSelection.length === 0) {
                    boxEl.style.display = 'none';
                    return;
                }

                let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
                currentSelection.forEach(id => {
                    const box = network.getBoundingBox(id);
                    if (box) {
                        minX = Math.min(minX, box.left);
                        minY = Math.min(minY, box.top);
                        maxX = Math.max(maxX, box.right);
                        maxY = Math.max(maxY, box.bottom);
                    }
                });

                if (minX === Infinity) {
                    boxEl.style.display = 'none';
                    return;
                }

                const p1 = network.canvasToDOM({ x: minX, y: minY });
                const p2 = network.canvasToDOM({ x: maxX, y: maxY });

                const padding = 20;
                boxEl.style.display = 'block';
                // selection-box is absolute in the relative container, so p1.x/p1.y are correct relative to canvas
                boxEl.style.left = (p1.x - padding) + 'px';
                boxEl.style.top = (p1.y - padding) + 'px';
                boxEl.style.width = (p2.x - p1.x + padding * 2) + 'px';
                boxEl.style.height = (p2.y - p1.y + padding * 2) + 'px';

                this.selectedNodes = [...currentSelection];

                if (this.selectedNodes.length > 1) {
                    this.updateSelectionMenu();
                }
            }, 0);
        },
        updateSelectionMenu() {
            const menuEl = document.getElementById('selection-menu');
            const boxEl = document.getElementById('selection-box');
            if (!menuEl || !boxEl) return;

            if (this.selectedNodes.length < 2 || boxEl.style.display === 'none') {
                return;
            }

            const rect = boxEl.getBoundingClientRect();
            menuEl.style.left = (rect.left + rect.width / 2) + 'px';
            menuEl.style.top = (rect.top - 40) + 'px';
        },
        getGroupStyle(group) {
            // This is now a fallback, manual updates happen in updateGroupOverlays
            if (this.showSheet) return 'display: none';
            const box = this.getGroupBox(group.nodeIds);
            if (!box) return 'display: none';

            const network = Alpine.raw(this.network);
            const canvas = document.getElementById('studio-canvas');
            if (!canvas) return 'display: none';

            const rect = canvas.getBoundingClientRect();
            // Align to the actual top-center of the group
            const p = network.canvasToDOM({ x: (box.minX + 80 + box.maxX - 80) / 2, y: box.minCanvasY + 80 });

            return `position: fixed; left: ${p.x + rect.left}px; top: ${p.y + rect.top - 45}px; z-index: 1500; pointer-events: auto; transform: translateX(-50%);`;
        },
        updateGroupOverlays() {
            if (this.showSheet) {
                this.groups.forEach(group => {
                    const el = document.querySelector(`[data-group-id="${group.id}"]`);
                    if (el) el.style.display = 'none';
                });
                return;
            }

            const canvas = document.getElementById('studio-canvas');
            if (!canvas) return;
            const rect = canvas.getBoundingClientRect();
            const network = Alpine.raw(this.network);
            if (!network) return;

            const compositeIds = this.hoveredGroupId && this.hoveredGroupId.includes(',') ? this.hoveredGroupId.split(',') : [];

            this.groups.forEach((group, idx) => {
                const el = document.querySelector(`[data-group-id="${group.id}"]`);
                if (!el) return;

                const isHovered = this.hoveredGroupId === group.id || compositeIds.includes(group.id);

                // If composite hover, only show the FIRST group element to avoid stacking
                const shouldShow = isHovered && (compositeIds.length === 0 || group.id === compositeIds[0]);

                if (shouldShow) {
                    const box = this.getGroupBox(group.nodeIds);
                    if (box) {
                        el.style.display = 'flex';
                        // Center horizontaly, position above group
                        const p = network.canvasToDOM({ x: (box.minX + 80 + box.maxX - 80) / 2, y: box.minCanvasY + 80 });
                        el.style.left = (p.x + rect.left) + 'px';
                        el.style.top = (p.y + rect.top - 45) + 'px';
                        el.style.transform = 'translateX(-50%)';
                        el.style.zIndex = "2000";
                    } else {
                        el.style.display = 'none';
                    }
                } else {
                    el.style.display = 'none';
                }
            });
        },
        addNodesToGroup(groupId) {
            const group = this.groups.find(g => g.id === groupId);
            if (!group) return;

            const newNodes = this.selectedNodes.filter(id => !group.nodeIds.includes(id));
            if (newNodes.length === 0) {
                toastr.info('No new nodes selected to add.');
                return;
            }

            // [VALIDATION] Check if adding these nodes would result in a duplicate group
            const nextNodeIds = [...group.nodeIds, ...newNodes];
            const duplicate = this.groups.find(g =>
                g.id !== groupId &&
                g.nodeIds.length === nextNodeIds.length &&
                g.nodeIds.every(id => nextNodeIds.includes(id))
            );

            if (duplicate) {
                toastr.warning(`Cannot add. This membership set already exists in "${duplicate.label}".`);
                return;
            }

            // Update nodes in dataset
            const nodesRaw = Alpine.raw(this.nodes);
            newNodes.forEach(id => {
                const node = nodesRaw.get(id);
                let currentGroups = Array.isArray(node.groupId) ? [...node.groupId] : (node.groupId ? [node.groupId] : []);
                let currentRisks = Array.isArray(node.groupRisk) ? [...node.groupRisk] : (node.groupRisk ? [node.groupRisk] : []);

                if (!currentGroups.includes(groupId)) {
                    currentGroups.push(groupId);
                    currentRisks.push(group.risk);
                }

                nodesRaw.update({ id, groupId: currentGroups, groupRisk: currentRisks });
            });

            group.nodeIds = [...group.nodeIds, ...newNodes];
            toastr.success(`Added ${newNodes.length} nodes to ${group.label}`);

            const network = Alpine.raw(this.network);
            this.updateGroupOverlays(); // Update UI badges
            network.redraw();
        },

        getActiveGroup() {
            if (this.selectedNodes.length < 2) return null;
            return this.groups.find(g =>
                g.nodeIds.length === this.selectedNodes.length &&
                g.nodeIds.every(id => this.selectedNodes.includes(id))
            );
        },
        dissolveGroup(id = null) {
            const groupId = id || (this.getActiveGroup() ? this.getActiveGroup().id : null);
            if (groupId) {
                const group = this.groups.find(g => g.id === groupId);
                if (group) {
                    const nodesRaw = Alpine.raw(this.nodes);
                    group.nodeIds.forEach(nodeId => {
                        const node = nodesRaw.get(nodeId);
                        if (node) {
                            const groupIdx = Array.isArray(node.groupId) ? node.groupId.indexOf(groupId) : -1;
                            let currentGroups = Array.isArray(node.groupId) ? node.groupId.filter(g => g !== groupId) : [];
                            let currentRisks = Array.isArray(node.groupRisk) ? node.groupRisk.filter((r, idx) => idx !== groupIdx) : [];

                            nodesRaw.update({
                                id: nodeId,
                                groupId: currentGroups.length > 0 ? currentGroups : null,
                                groupRisk: currentRisks.length > 0 ? currentRisks : null
                            });
                        }
                    });
                }
                this.groups = this.groups.filter(g => g.id !== groupId);
                toastr.success('Group dissolved successfully.');
                const network = Alpine.raw(this.network);
                network.redraw();
            }
        },
        openGroupSettings(groupId) {
            console.log('Opening Group Settings for:', groupId);
            if (!groupId || !this.groups.find(g => g.id === groupId)) {
                console.warn('Group not found:', groupId);
                return;
            }
            this.showSheet = false; // Close property panel if open
            this.activeGroupId = groupId;
            this.selectedGroupMembers = []; // Reset selection when opening
            this.isGroupModalOpen = true;
            this.$nextTick(() => {
                if (window.lucide) lucide.createIcons();
            });
        },
        openGroupManager() {
            console.log('Opening Group Manager for selected node');
            if (!this.selectedNode) {
                console.warn('No node selected');
                return;
            }
            const nodeGroups = Array.isArray(this.selectedNode.groupId)
                ? this.selectedNode.groupId
                : (this.selectedNode.groupId ? [this.selectedNode.groupId] : []);

            if (nodeGroups.length === 0) {
                console.warn('Selected node has no groups');
                return;
            }

            this.showSheet = false; // Close property panel if open
            this.activeGroupId = nodeGroups[0] || null;
            this.isGroupModalOpen = true;
        },
        removeNodesFromGroup(groupId, nodeIds = null) {
            const targets = nodeIds || (this.selectedGroupMembers.length > 0 ? [...this.selectedGroupMembers] : [...this.selectedNodes]);
            if (targets.length === 0) {
                toastr.warning('Please select nodes to remove.');
                return;
            }

            const group = this.groups.find(g => g.id === groupId);
            if (!group) return;

            const nodesRaw = Alpine.raw(this.nodes);
            targets.forEach(nodeId => {
                const node = nodesRaw.get(nodeId);
                if (node) {
                    let currentGroups = Array.isArray(node.groupId) ? node.groupId.filter(g => g !== groupId) : [];
                    let currentRisks = Array.isArray(node.groupRisk) ? node.groupRisk.filter((r, idx) => node.groupId[idx] !== groupId) : [];

                    nodesRaw.update({
                        id: nodeId,
                        groupId: currentGroups.length > 0 ? currentGroups : null,
                        groupRisk: currentRisks.length > 0 ? currentRisks : null
                    });
                    group.nodeIds = group.nodeIds.filter(id => id !== nodeId);
                }
            });

            this.selectedGroupMembers = [];

            if (group.nodeIds.length < 2) {
                this.dissolveGroup(groupId);
                this.isGroupModalOpen = false;
            }

            this.updateGroupOverlays();
            const network = Alpine.raw(this.network);
            network.redraw();
            toastr.success(`${targets.length} nodes removed from group.`);
        },

        toggleGroupMembership(groupId) {
            if (!this.selectedNode) return;
            const nodeId = this.selectedNode.id;
            const nodesRaw = Alpine.raw(this.nodes);
            const node = nodesRaw.get(nodeId);
            if (!node) return;

            let currentGroups = Array.isArray(node.groupId) ? [...node.groupId] : (node.groupId ? [node.groupId] : []);
            let currentRisks = Array.isArray(node.groupRisk) ? [...node.groupRisk] : (node.groupRisk ? [node.groupRisk] : []);

            const group = this.groups.find(g => g.id === groupId);
            if (!group) return;

            if (currentGroups.includes(groupId)) {
                // Remove
                const idx = currentGroups.indexOf(groupId);
                currentGroups.splice(idx, 1);
                currentRisks.splice(idx, 1);
                group.nodeIds = group.nodeIds.filter(id => id !== nodeId);
            } else {
                // Add
                // [VALIDATION] Check if adding this node would result in a duplicate group
                const nextNodeIds = [...group.nodeIds, nodeId];
                const duplicate = this.groups.find(g =>
                    g.id !== groupId &&
                    g.nodeIds.length === nextNodeIds.length &&
                    g.nodeIds.every(id => nextNodeIds.includes(id))
                );

                if (duplicate) {
                    toastr.warning(`Cannot add. This membership set already exists in "${duplicate.label}".`);
                    return;
                }

                currentGroups.push(groupId);
                currentRisks.push(group.risk);
                if (!group.nodeIds.includes(nodeId)) group.nodeIds.push(nodeId);
            }

            nodesRaw.update({
                id: nodeId,
                groupId: currentGroups.length > 0 ? currentGroups : null,
                groupRisk: currentRisks.length > 0 ? currentRisks : null
            });

            // Update local node reference
            this.selectedNode.groupId = currentGroups.length > 0 ? currentGroups : null;
            this.selectedNode.groupRisk = currentRisks.length > 0 ? currentRisks : null;
            this.formData.groupId = this.selectedNode.groupId;

            // Handle group dissolution if empty
            if (group.nodeIds.length < 2 && currentGroups.includes(groupId) === false) {
                // Trigger dissolve logic if this removal made it < 2
                this.dissolveGroup(groupId);
            }

            this.updateGroupOverlays();
            const network = Alpine.raw(this.network);
            network.redraw();
        },

        toggleGroupMemberSelection(nodeId) {
            if (this.selectedGroupMembers.includes(nodeId)) {
                this.selectedGroupMembers = this.selectedGroupMembers.filter(id => id !== nodeId);
            } else {
                this.selectedGroupMembers.push(nodeId);
            }
        },

        deleteGroupNodes(id = null) {
            const nodesToDelete = this.selectedGroupMembers.length > 0
                ? [...this.selectedGroupMembers]
                : (id ? [...this.groups.find(g => g.id === id).nodeIds] : [...this.selectedNodes]);

            if (nodesToDelete.length === 0) {
                toastr.warning('Please select nodes to delete.');
                return;
            }

            const screened = nodesToDelete.filter(nodeId => {
                const node = this.nodes.get(nodeId);
                return node && node.isScreened;
            });

            if (screened.length > 0) {
                toastr.warning('Some nodes are screened and locked. Only unscreened nodes will be deleted.');
            }

            const targetNodes = nodesToDelete.filter(nodeId => {
                const node = this.nodes.get(nodeId);
                return node && !node.isScreened;
            });

            if (targetNodes.length > 0) {
                this.removeNodesAndEdges(targetNodes);
                // Clear group membership for these nodes from ALL groups
                this.groups.forEach(g => {
                    g.nodeIds = g.nodeIds.filter(nid => !targetNodes.includes(nid));
                });
                // Remove groups that now have < 2 members
                this.groups.filter(g => g.nodeIds.length < 2).forEach(g => this.dissolveGroup(g.id));

                if (!id) this.selectedNodes = [];
                this.selectedGroupMembers = [];
                this.updateBoundingBox();
                toastr.success(`${targetNodes.length} nodes removed.`);

                // If the active group was deleted or emptied, close modal
                if (id && (!this.groups.find(g => g.id === id) || this.groups.find(g => g.id === id).nodeIds.length === 0)) {
                    this.isGroupModalOpen = false;
                }
            }
        },
        async renameGroup(id = null) {
            const group = id ? this.groups.find(g => g.id === id) : this.getActiveGroup();
            if (!group) return;

            const { value: newName } = await Swal.fire({
                title: 'Rename Group',
                input: 'text',
                inputValue: group.label,
                showCancelButton: true,
                inputValidator: (value) => {
                    if (!value) return 'You need to write something!';
                }
            });

            if (newName) {
                group.label = newName;
                toastr.success('Group renamed.');
                const network = Alpine.raw(this.network);
                network.redraw();
            }
        },
        autoLayout(type = 'adaptive', silent = false) {
            const network = Alpine.raw(this.network);
            const nodes = this.nodes.get();
            const edges = this.edges.get();
            
            // Sync all node visuals in case relationships were updated from bulk loads or initial render
            nodes.forEach(n => this.updateNodeVisuals(n.id));

            if (network) network.setOptions({ physics: { enabled: true }, layout: { hierarchical: { enabled: false } } });

            if (type === 'hierarchical') {
                const levels = {};
                nodes.forEach(n => {
                    const hasParent = edges.some(e => e.to === n.id);
                    if (!hasParent || n.isMainEntity || n.isRoot) {
                        levels[n.id] = 0;
                    }
                });

                let queue = Object.keys(levels);
                while (queue.length > 0) {
                    const currentId = queue.shift();
                    const currentLevel = levels[currentId];
                    edges.filter(e => e.from === currentId).forEach(edge => {
                        const toNode = nodes.find(n => n.id === edge.to);
                        const isMain = toNode && (toNode.isMainEntity || toNode.isRoot);
                        if (!isMain) {
                            levels[edge.to] = Math.max(levels[edge.to] || 0, currentLevel + 1);
                            queue.push(edge.to);
                        }
                    });
                }

                const nodesRaw = Alpine.raw(this.nodes);
                nodesRaw.update(nodes.map(n => ({ id: n.id, level: levels[n.id] || 0, x: undefined, y: undefined })));
                this.activeLayout = 'hierarchical';

                network.setOptions({
                    layout: {
                        hierarchical: {
                            enabled: true,
                            levelSeparation: 250,
                            nodeSpacing: 300,
                            treeSpacing: 300,
                            blockShifting: true,
                            edgeMinimization: true,
                            parentCentralization: true,
                            direction: 'UD',
                            sortMethod: 'hubsize'
                        }
                    },
                    edges: {
                        smooth: false,
                        font: { align: 'middle', size: 11, face: 'Inter', strokeWidth: 4, strokeColor: '#ffffff', color: '#64748B', weight: 'bold' },
                        arrows: { from: { enabled: true, scaleFactor: 0.5 } }
                    },
                    physics: {
                        enabled: true,
                        solver: 'hierarchicalRepulsion',
                        hierarchicalRepulsion: {
                            nodeDistance: 120, // Tighter placement
                            centralGravity: 0.0,
                            springLength: 100,
                            springConstant: 0.01,
                            damping: 0.09
                        },
                        stabilization: {
                            enabled: true,
                            iterations: 1000,
                            updateInterval: 25
                        }
                    }
                });

                // Disable physics after stabilization to allow manual dragging without pushing other nodes too far
                network.once('stabilized', () => {
                    network.setOptions({ physics: { enabled: false } });
                    if (!silent) toastr.success('Hierarchy stabilized.', 'Layout');
                });

            } else {
                if (this.activeLayout !== 'adaptive') {
                    const nodesRaw = Alpine.raw(this.nodes);
                    nodesRaw.update(nodes.map(n => ({ id: n.id, level: null, x: undefined, y: undefined })));
                }
                this.activeLayout = 'adaptive';

                network.setOptions({
                    layout: { hierarchical: { enabled: false }, improvedLayout: true },
                    edges: {
                        smooth: { type: 'continuous', roundness: 0.5 },
                        font: { align: 'middle', size: 11, face: 'Inter', strokeWidth: 4, strokeColor: '#ffffff', color: '#64748B', weight: 'bold' }
                    },
                    physics: {
                        enabled: true,
                        solver: 'forceAtlas2Based',
                        forceAtlas2Based: {
                            gravitationalConstant: -150,
                            centralGravity: 0.01,
                            springLength: 150,
                            springConstant: 0.08,
                            damping: 0.4,
                            avoidOverlap: 1
                        },
                        stabilization: { iterations: 1000 }
                    }
                });
            }

            network.once('stabilizationIterationsDone', () => {
                network.setOptions({
                    physics: { enabled: false },
                    // Do NOT disable hierarchical layout if we want it to stay a rigid tree
                    interaction: {
                        dragView: true,
                        zoomView: true,
                        dragNodes: this.activeTool !== 'hand',
                        selectable: this.activeTool !== 'hand'
                    }
                });
                if (!silent) network.fit({ animation: { duration: 800 } });
            });

            if (this.activeTool === 'connect') this.setTool('pointer');

            network.stabilize();
            if (!silent) toastr.info(`Applying ${type} layout...`, 'Layout Update');
        },
        setTool(tool) {
            this.activeTool = tool;
            const canvas = document.getElementById('studio-canvas');
            const network = Alpine.raw(this.network);
            canvas.classList.remove('cursor-grab', 'cursor-crosshair');

            if (tool === 'hand') {
                canvas.classList.add('cursor-grab');
                network.unselectAll();
                this.selectedNodes = [];
                this.closeSheet();
                this.updateBoundingBox();
                network.setOptions({
                    interaction: {
                        dragView: true,
                        zoomView: true,
                        dragNodes: false,
                        selectable: false,
                        multiselect: false
                    }
                });
            } else if (tool === 'pointer') {
                network.setOptions({
                    interaction: {
                        dragView: true,
                        zoomView: true,
                        dragNodes: true,
                        selectable: true,
                        multiselect: true
                    }
                });
            } else if (tool === 'connect') {
                canvas.classList.add('cursor-crosshair');
                network.setOptions({
                    interaction: {
                        dragView: false,
                        zoomView: true,
                        dragNodes: false,
                        selectable: true
                    }
                });
                toastr.info('Click a source node to start connecting.', 'Link Mode');
            }
        },
        fitView() {
            const network = Alpine.raw(this.network);
            if (network) {
                network.fit({
                    animation: {
                        duration: 800,
                        easingFunction: 'easeInOutQuad'
                    }
                });
            }
        },
        renderGroups(ctx) {
            if (!this.groups || this.groups.length === 0) return;
            const network = Alpine.raw(this.network);
            const positions = network.getPositions();

            // Distinguishable colors for multiple groups
            const groupColors = ['#2563EB', '#10B981', '#F59E0B', '#8B5CF6', '#EC4899', '#06B6D4', '#6366F1'];

            this.groups.forEach((group, gIdx) => {
                ctx.save();
                const groupColor = groupColors[gIdx % groupColors.length];

                group.nodeIds.forEach(nodeId => {
                    const pos = positions[nodeId];
                    if (pos) {
                        // Background glow for grouped nodes - Subtler, soft-light approach
                        const rgbaSubtle = this.hexToRgba(groupColor, 0.08);

                        const gradient = ctx.createRadialGradient(pos.x, pos.y, 30, pos.x, pos.y, 75);
                        gradient.addColorStop(0, rgbaSubtle);
                        // gradient.addColorStop(1, 'transparent');

                        ctx.beginPath();
                        ctx.arc(pos.x, pos.y, 75, 0, Math.PI * 2);
                        ctx.fillStyle = gradient;
                        ctx.fill();

                        // Multi-Group Indicator - Render multiple dots if node belongs to multiple groups
                        const node = network.body.nodes[nodeId];
                        const groupIds = Array.isArray(node.options.groupId) ? node.options.groupId : (node.options.groupId ? [node.options.groupId] : []);

                        groupIds.forEach((gid, idx) => {
                            const gIdx = this.groups.findIndex(g => g.id === gid);
                            const activeColor = groupColors[gIdx % groupColors.length] || groupColor;

                            // Offset dots for multiple groups
                            const angle = (idx * (Math.PI / 4)) + (Math.PI / 4);
                            const offsetX = Math.cos(angle) * 45;
                            const offsetY = Math.sin(angle) * 45;

                            ctx.shadowColor = 'rgba(0,0,0,0.1)';
                            ctx.shadowBlur = 4;
                            ctx.shadowOffsetX = 0;
                            ctx.shadowOffsetY = 2;

                            ctx.fillStyle = activeColor;
                            ctx.beginPath();
                            ctx.arc(pos.x + offsetX, pos.y + offsetY, 14, 0, Math.PI * 2);
                            ctx.fill();

                            ctx.shadowColor = 'transparent';
                            ctx.fillStyle = '#FFFFFF';
                            ctx.font = 'black 12px Inter';
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';
                            ctx.fillText('G' + (gIdx + 1), pos.x + offsetX, pos.y + offsetY);
                        });
                    }
                });
                ctx.restore();
            });
        },
        hexToRgba(hex, alpha) {
            let r = 0, g = 0, b = 0;
            if (hex.length === 4) {
                r = parseInt(hex[1] + hex[1], 16);
                g = parseInt(hex[2] + hex[2], 16);
                b = parseInt(hex[3] + hex[3], 16);
            } else if (hex.length === 7) {
                r = parseInt(hex.substring(1, 3), 16);
                g = parseInt(hex.substring(3, 5), 16);
                b = parseInt(hex.substring(5, 7), 16);
            }
            return `rgba(${r}, ${g}, ${b}, ${alpha})`;
        },
        getCompactGroupRange(groupIds) {
            if (!groupIds) return '';
            const ids = (Array.isArray(groupIds) ? groupIds : [groupIds])
                .map(gid => this.groups.findIndex(g => g.id === gid) + 1)
                .filter(idx => idx > 0)
                .sort((a, b) => a - b);

            if (ids.length === 0) return '';
            if (ids.length <= 3) return 'G: ' + ids.join(', ');

            const ranges = [];
            let start = ids[0];
            let end = ids[0];

            for (let i = 1; i <= ids.length; i++) {
                if (i < ids.length && ids[i] === end + 1) {
                    end = ids[i];
                } else {
                    if (start === end) {
                        ranges.push(start);
                    } else if (end === start + 1) {
                        ranges.push(start, end);
                    } else {
                        ranges.push(`${start}-${end}`);
                    }
                    if (i < ids.length) {
                        start = ids[i];
                        end = ids[i];
                    }
                }
            }
            return 'G: ' + ranges.join(', ');
        },
        renderSelectionMarquee(ctx) {
            if (this.selectedNodes.length < 2) return;
            const box = this.getGroupBox(this.selectedNodes);
            if (!box) return;

            ctx.save();
            ctx.setLineDash([6, 4]);
            ctx.lineDashOffset = -this.marqueeOffset;
            ctx.strokeStyle = '#2563EB';
            ctx.lineWidth = 2;

            ctx.beginPath();
            ctx.roundRect(box.minX, box.minY, box.width, box.height, 24);
            ctx.stroke();

            ctx.fillStyle = 'rgba(37, 99, 235, 0.05)';
            ctx.fill();
            ctx.restore();
        },
        async groupEntities() {
            if (this.selectedNodes.length < 2) {
                toastr.warning('Please select at least 2 nodes to create a group.');
                return;
            }

            // [VALIDATION] Check if a group with EXACTLY these nodes already exists (Strict Subset/Superset check)
            const duplicate = this.groups.find(g =>
                g.nodeIds.length === this.selectedNodes.length &&
                g.nodeIds.every(id => this.selectedNodes.includes(id))
            );

            if (duplicate) {
                toastr.warning(`A group with this exact selection already exists in "${duplicate.label}".`);
                return;
            }

            const { value: groupName } = await Swal.fire({
                title: 'Create New Group',
                input: 'text',
                inputLabel: 'Group Name',
                inputValue: `Group ${this.groups.length + 1}`,
                showCancelButton: true,
                inputValidator: (value) => {
                    if (!value) return 'You need to write something!';
                }
            });

            if (!groupName) return;

            const groupId = 'G' + Date.now();
            const newGroup = {
                id: groupId,
                nodeIds: [...this.selectedNodes],
                label: groupName,
                risk: 'Unclassified'
            };

            this.groups.push(newGroup);

            // Update nodes in dataset to include this new group
            const nodesRaw = Alpine.raw(this.nodes);
            this.selectedNodes.forEach(id => {
                const node = nodesRaw.get(id);
                let currentGroups = Array.isArray(node.groupId) ? [...node.groupId] : (node.groupId ? [node.groupId] : []);
                let currentRisks = Array.isArray(node.groupRisk) ? [...node.groupRisk] : (node.groupRisk ? [node.groupRisk] : []);

                if (!currentGroups.includes(groupId)) {
                    currentGroups.push(groupId);
                    currentRisks.push(newGroup.risk);
                }

                nodesRaw.update({ id, groupId: currentGroups, groupRisk: currentRisks });
            });

            toastr.success(`Group "${groupName}" created successfully.`);

            const network = Alpine.raw(this.network);
            this.updateGroupOverlays();
            network.redraw();
        },

        resetFormData() {
            return {
                id: '',
                caseId: '',
                name: '',
                type: 'I',
                isRoot: false,
                isMainEntity: false,
                nationality: '',
                dob: '',
                gender: '',
                profession: '',
                residence: '',
                placeOfBirth: '',
                cif: '',
                idType: '',
                idNumber: '',
                tradeLicence: '',
                tradeLicenseAuthority: '',
                registrationDate: '',
                entityTypeTxt: '',
                businessType: '',
                employer: '',
                employerIndustry: '',
                employerSector: '',
                sowsofcountry: '',
                goldenVisa: 'No',
                selectedIdTypes: [],
                passportId: '',
                passportIssueDate: '',
                passportExpiryDate: '',
                emiratesIdNumber: '',
                emiratesIdIssueDate: '',
                emiratesIdExpiryDate: '',
                productType: '',
                productValue: '',
                shareholderType: '',
                deliveryChannel: '',
                paymentMode: '',
                counterParty: '',
                counterPartyName: '',
                productRefNo: '',
                relationship: '',
                share: 0,
                designation: '',
                threshold: 70,
                matchScore: 0,
                isScreened: false,
                attachments: [],
                screeningSources: ['1'],
                isOCRMode: false,
                isBulkMode: false,
                showScreeningDropdown: false,
                matchParameter: 85
            };
        },
        formatDateForInput(dateStr) {
            if (!dateStr || dateStr.startsWith('0001-01-01')) return '';
            try {
                const date = new Date(dateStr);
                if (isNaN(date.getTime())) return '';

                const d = date.getDate().toString().padStart(2, '0');
                const m = (date.getMonth() + 1).toString().padStart(2, '0');
                const y = date.getFullYear();
                return `${d}/${m}/${y}`;
            } catch (e) {
                return '';
            }
        },
        getNumericRisk(risk) {
            if (!risk) return 0;
            const r = risk.toLowerCase();
            if (r.includes('high')) return 3;
            if (r.includes('medium')) return 2;
            if (r.includes('low')) return 1;
            return 0;
        },
        getRiskFromNumeric(num) {
            if (num === 3) return 'High Risk';
            if (num === 2) return 'Medium Risk';
            if (num === 1) return 'Low Risk';
            return 'Unclassified';
        },
        normalizeType(rawType) {
            if (!rawType || typeof rawType !== 'string') return 'I';
            const parts = rawType.toUpperCase().split('_');
            const lastPart = parts[parts.length - 1];

            if (lastPart === 'C' || lastPart === 'CORP' || lastPart === 'CORPORATE') return 'C';
            if (lastPart === 'I' || lastPart === 'IND' || lastPart === 'INDIV' || lastPart === 'INDIVIDUAL') return 'I';

            // Fallback for cases like 'Corporate' (no underscore) or other starts
            if (rawType.toUpperCase().startsWith('CORP')) return 'C';
            return 'I';
        },

        zoomIn() {
            const network = Alpine.raw(this.network);
            network.moveTo({ scale: network.getScale() * 1.2, animation: true });
        },
        zoomOut() {
            const network = Alpine.raw(this.network);
            network.moveTo({ scale: network.getScale() * 0.8, animation: true });
        },
        toggleCollapse(nodeId, e) {
            e.stopPropagation();
            const id = nodeId.toString();
            if (this.collapsedNodes.includes(id)) {
                this.collapsedNodes = this.collapsedNodes.filter(x => x !== id);
            } else {
                this.collapsedNodes.push(id);
            }
            this.updateTree();
        },
        centerNode(nodeId, e = null, shouldOpen = true) {
            const network = Alpine.raw(this.network);
            if (!network) return;
            const targetId = nodeId.toString();
            const currentSelection = network.getSelectedNodes().map(id => id.toString());
            const isSelected = currentSelection.includes(targetId);

            let nextSelection = [];
            if (e && (e.ctrlKey || e.shiftKey)) {
                if (isSelected) {
                    nextSelection = currentSelection.filter(id => id !== targetId);
                } else {
                    nextSelection = [...currentSelection, targetId];
                }
            } else {
                nextSelection = [targetId];
            }

            network.setSelection({ nodes: nextSelection });
            this.selectedNodes = [...nextSelection];
            this.updateBoundingBox();

            // Always center the target node on explicit click from sidebar/tree
            const pos = network.getPositions([targetId])[targetId];
            if (pos) {
                network.moveTo({
                    position: { x: pos.x, y: pos.y },
                    scale: network.getScale() < 0.5 ? 0.8 : network.getScale(), // Zoom in if too far, otherwise keep scale
                    animation: { duration: 500, easingFunction: 'easeInOutQuad' }
                });
            }
            const nodes = Alpine.raw(this.nodes);
            const node = nodes.get(targetId);
            if (node && shouldOpen) this.openNode(node);
        },
        isLastChild(index) {
            if (index === this.treeNodes.length - 1) return true;
            return this.treeNodes[index + 1].depth < this.treeNodes[index].depth;
        },

        /**
         * Recursively updates shareholderType for a node and all its descendants
         * following the legacy underscore pattern: PARENT_TYPE + "_" + CURRENT_BASE
         */
        updateHierarchyTypes(nodeId) {
            const nodes = Alpine.raw(this.nodes);
            const edges = Alpine.raw(this.edges);
            const node = nodes.get(nodeId);
            if (!node) return;

            const incomingEdges = edges.get({ filter: e => e.to.toString() === nodeId.toString() });
            let parentId = null;
            if (incomingEdges.length > 0) {
                const parentNodes = incomingEdges.map(e => nodes.get(e.from)).filter(Boolean);
                const mainParent = parentNodes.find(p => p.isMainEntity || p.isRoot);
                parentId = mainParent ? mainParent.id : incomingEdges[0].from;
            }
            const parent = parentId ? nodes.get(parentId) : null;

            let newType = '';
            const base = node.type === 'C' ? 'Corp' : 'Ind';

            if (node.isMainEntity) {
                newType = node.type === 'C' ? 'Corporate' : 'Individual';
            } else if (parent) {
                // Legacy pattern: ParentType_Ind or ParentType_Corp
                let parentType = parent.shareholderType;
                if (parent.isMainEntity) {
                    parentType = parent.type === 'C' ? 'Corporate' : 'Individual';
                } else if (!parentType) {
                    parentType = parent.type === 'C' ? 'Corporate' : 'Individual';
                }
                newType = `${parentType}_${base}`;
            } else {
                // Root nodes use full labels
                newType = node.type === 'C' ? 'Corporate' : 'Individual';
            }

            // Limit nesting to prevent database overflow (Legacy parity)
            if ((newType.match(/_/g) || []).length > 7) return;

            let flagType = this.getRelationshipLabel(node.relationship);
            if (node.isRoot && !flagType) {
                flagType = node.type === 'C' ? 'Corporate' : 'Individual';
            }

            // Compute relationships for SVG image (avoiding single relationship overwrite)
            const relationships = incomingEdges.map(e => e.relationship).filter(Boolean);
            let relsToPass = [];
            if (relationships.length > 0) {
                relsToPass = relationships;
            } else if (!node.isRoot && node.relationship) {
                relsToPass = [node.relationship];
            }

            nodes.update({ 
                id: nodeId, 
                shareholderType: newType, 
                flagType: flagType,
                image: this.getSvg(node.type || 'C', node.isRoot, node.isScreened, node.isDuplicate, node.caseId, relsToPass, !!(node.isMainEntity || node.isRoot))
            });

            // Sync current form if this is the active node
            if (this.selectedNode && this.selectedNode.id === nodeId) {
                this.formData.shareholderType = newType;
                this.formData.flagType = flagType;
            }

            // Recursively update all children
            const childrenEdges = edges.get({ filter: e => e.from.toString() === nodeId.toString() });
            childrenEdges.forEach(edge => {
                this.updateHierarchyTypes(edge.to);
            });
        },

        getTreeNodes() {
            const nodes = this.nodes.get();
            const edges = this.edges.get();
            const tree = [];
            // Track visited edge keys (not node ids) so a node can appear under multiple parents
            const visitedEdges = new Set();
            const addedRootIds = new Set();

            const topLevelNodes = nodes.filter(n => !edges.some(e => e.to === n.id) || n.isMainEntity)
                .sort((a, b) => {
                    // Prioritize main entity and root nodes first
                    const aIsMainOrRoot = !!(a.isMainEntity || a.isRoot);
                    const bIsMainOrRoot = !!(b.isMainEntity || b.isRoot);
                    
                    if (aIsMainOrRoot && !bIsMainOrRoot) return -1;  // a comes first
                    if (!aIsMainOrRoot && bIsMainOrRoot) return 1;   // b comes first
                    
                    // If both or neither are main/root, sort alphabetically by name or id
                    return (a.name || a.label || a.id.toString()).localeCompare(b.name || b.label || b.id.toString());
                });

            const processNode = (node, depth = 0, isParentVisible = true, parentEdge = null) => {
                // Key by EDGE (from->to), not by node id, so same child can appear under multiple parents
                const edgeKey = parentEdge ? `${parentEdge.from}->${parentEdge.to}@${parentEdge.id || ''}` : `root->${node.id}`;
                if (visitedEdges.has(edgeKey)) return;
                visitedEdges.add(edgeKey);

                const label = node.label || '';
                const isMatch = this.appliedSearchQuery && label.toLowerCase().includes(this.appliedSearchQuery.toLowerCase());
                
                let displayShType = node.shareholderType || this.getTypeLabel(node.type);
                if (node.isMainEntity) {
                    displayShType = node.type === 'C' ? 'Corporate' : 'Individual';
                } else if (parentEdge) {
                    const parentNode = nodes.find(n => n.id === parentEdge.from);
                    if (parentNode) {
                        const parentIsCorp = parentNode.type === 'C';
                        const childIsCorp = node.type === 'C';
                        displayShType = (parentIsCorp ? 'Corporate_' : 'Individual_') + (childIsCorp ? 'Corp' : 'Ind');
                    }
                }
                
                // Use a unique tree key combining node id + parent edge context
                tree.push({ 
                    ...node, 
                    treeKey: parentEdge ? `${node.id}-${parentEdge.from}` : `${node.id}-root`,
                    depth, 
                    isMatch, 
                    isVisible: isParentVisible,
                    displayTypeLabel: displayShType
                });

                const isCollapsed = this.collapsedNodes.includes(node.id.toString());
                const outgoingEdges = edges.filter(e => e.from === node.id);
                
                outgoingEdges.forEach(edge => {
                    const childNode = nodes.find(n => n.id === edge.to);
                    if (childNode) {
                        processNode(childNode, depth + 1, isParentVisible && !isCollapsed, edge);
                    }
                });
            };

            topLevelNodes.forEach(n => {
                addedRootIds.add(n.id);
                processNode(n, 0);
            });
            nodes.forEach(n => {
                if (!addedRootIds.has(n.id) && !visitedEdges.has(`root->${n.id}`) && !edges.some(e => e.to === n.id)) {
                    processNode(n, 0);
                }
            });

            return tree.filter(n => n.isVisible);
        },
        dragStartTree(e, id) {
            this.draggedTreeId = id;
            e.dataTransfer.setData('text/plain', id); // Required for drag to start
            e.dataTransfer.effectAllowed = 'move';
            e.target.closest('.tree-item').classList.add('opacity-40');
        },
        dragOverTree(e, id) {
            if (this.draggedTreeId === id) return;
            const el = e.currentTarget;
            el.classList.add('bg-blue-50', 'border-blue-200');
        },
        dragLeaveTree(e) {
            const el = e.currentTarget;
            el.classList.remove('bg-blue-50', 'border-blue-200');
        },
        dropTree(e, targetId) {
            const el = e.currentTarget;
            el.classList.remove('bg-blue-50', 'border-blue-200');
            document.querySelectorAll('.tree-item').forEach(item => item.classList.remove('opacity-40'));

            if (!this.draggedTreeId || this.draggedTreeId === targetId) return;

            const sourceId = this.draggedTreeId.toString();
            const target = targetId.toString();

            const sourceNode = this.nodes.get(sourceId);
            if (sourceNode && sourceNode.isScreened) {
                toastr.warning('Screened entities are locked and cannot be re-parented.');
                return;
            }

            // Circular dependency check: Can source reach target?
            if (this.canReach(sourceId, target)) {
                toastr.error('Circular relationships are not allowed.');
                return;
            }

            // Reparent / Add Parent: 
            // Removed constraint: Allow multiple parents
            // const incomingEdges = this.edges.get().filter(e => e.to.toString() === sourceId);
            // this.edges.remove(incomingEdges);

            // 2. Remove any edge that might exist in the opposite direction (target -> source is what we want, so remove source -> target)
            const edgesRaw = Alpine.raw(this.edges);
            const oppositeEdges = edgesRaw.get().filter(e => e.from.toString() === sourceId && e.to.toString() === target);
            edgesRaw.remove(oppositeEdges);

            // 3. Add the new edge: Parent -> Child
            edgesRaw.add({ from: target, to: sourceId });

            const nodesRaw = Alpine.raw(this.nodes);
            nodesRaw.update({ id: sourceId, isRoot: false });

            this.updateHierarchyTypes(sourceId);

            if (this.activeLayout === 'hierarchical') {
                this.autoLayout('hierarchical', true);
            } else {
                // Adaptive mode: Move child node near parent to avoid long edges
                const network = Alpine.raw(this.network);
                const parentPos = network.getPositions([target])[target];
                if (parentPos) {
                    this.nodes.update({
                        id: sourceId,
                        x: parentPos.x + (Math.random() * 40 - 20),
                        y: parentPos.y + 150
                    });
                }
            }
            this.updateTree();
            toastr.success('Relationship updated.');
        },

        dropOnRoot(e) {
            e.currentTarget.classList.remove('border-blue-400', 'bg-blue-50');
            document.querySelectorAll('.tree-item').forEach(item => item.classList.remove('opacity-40'));

            const sourceId = this.draggedTreeId.toString();
            const sourceNode = this.nodes.get(sourceId);
            if (sourceNode && sourceNode.isScreened) {
                toastr.warning('Screened entities are locked and cannot be modified.');
                return;
            }

            const edgesRaw = Alpine.raw(this.edges);
            const incomingEdges = edgesRaw.get().filter(e => e.to === sourceId);
            edgesRaw.remove(incomingEdges);

            const nodesRaw = Alpine.raw(this.nodes);
            nodesRaw.update({ id: sourceId, isRoot: true, isMainEntity: true });

            this.updateHierarchyTypes(sourceId);

            if (this.activeLayout === 'hierarchical') {
                this.autoLayout('hierarchical', true);
            }
            this.updateTree();
            toastr.success('Promoted to Main Entity.');
        },
        applySearch() {
            this.appliedSearchQuery = this.searchQuery;

            // Logic to highlight and center the first match
            const q = this.searchQuery.toLowerCase().trim();
            if (!q) return;

            const nodes = Alpine.raw(this.nodes);
            const firstMatch = nodes.get().find(n =>
                n.label.toLowerCase().includes(q) ||
                (n.cif && n.cif.toLowerCase().includes(q))
            );

            if (firstMatch) {
                this.centerNode(firstMatch.id);
                this.$nextTick(() => {
                    const el = document.querySelector(`[data-node-id="${firstMatch.id}"]`);
                    if (el) {
                        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        el.classList.add('search-pulse');
                        setTimeout(() => el.classList.remove('search-pulse'), 2000);
                    }
                });
            }
        },
        async searchFetch() {
            if (this.fetchQuery.length === 0) {
                this.fetchResults = [];
                this.isHistoryMode = false;
                return;
            }
            if (this.fetchQuery.length < 2) {
                this.fetchResults = [...this.fetchHistory];
                this.isHistoryMode = true;
                return;
            }

            try {
                const response = await fetch('/case/AutoCompleteCustomer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: new URLSearchParams({ prefix: this.fetchQuery })
                });
                const data = await response.json();

                // Check for exact match by CIF, CustomerID or ReferenceID (ignoring spaces for exact check)
                const query = this.fetchQuery.toLowerCase().trim();
                const normalizedQuery = query.replace(/\s+/g, '');

                if (data && data.length > 0) {
                    // We have matches from server (already limited to 3)
                    this.fetchResults = data;
                    this.isHistoryMode = false;
                } else {
                    // No matches from server - show recent searches history
                    this.fetchResults = this.fetchHistory.slice(0, 3);
                    this.isHistoryMode = true;
                }

                this.$nextTick(() => {
                    if (window.lucide) window.lucide.createIcons();
                });
            } catch (e) {
                console.error('Fetch error:', e);
                this.fetchResults = [...this.fetchHistory];
                this.isHistoryMode = true;
            }
        },
        getDisplayName(item) {
            if (!item) return 'Unknown';
            let name = item.fullName || item.FullName || item.onb_name || '';
            if (!name || name.toLowerCase() === 'null') {
                const first = item.firstName || item.FirstName || '';
                const last = item.lastName || item.LastName || '';
                name = (first + ' ' + last).trim();
            }
            return name || item.customerId || item.CustomerId || 'Unknown Entity';
        },
        getCaseID(item) {
            if (!item) return '';
            return item.caseId || item.CaseId || item.customercode || item.customerCode || item.cifNumber || item.CIFNumber || item.customerId || item.CustomerId || 'No Identifier';
        },
        getDisplayNationality(item) {
            if (!item) return '';
            return item.nationality || item.Nationality || item.onb_nationality || item.country || item.Country || 'Unknown Nationality';
        },
        loadHistory() {
            const stored = localStorage.getItem('studio_fetch_history');
            if (stored) {
                try {
                    this.fetchHistory = JSON.parse(stored);
                } catch (e) {
                    this.fetchHistory = [];
                }
            }
        },
        addToHistory(item) {
            // Keep only last 3, unique by CaseID
            const id = this.getCaseID(item);
            let history = this.fetchHistory.filter(i => this.getCaseID(i) !== id);
            history.unshift(item);
            this.fetchHistory = history.slice(0, 3);
            localStorage.setItem('studio_fetch_history', JSON.stringify(this.fetchHistory));
        },
        async confirmFetch(query) {
            if (!query) return;

            // If results are empty but query exists, try one last direct search to avoid "double click" issue
            if (this.fetchResults.length === 0 && query.length >= 3) {
                await this.searchFetch();
            }

            if (this.fetchResults.length > 0) {
                // If we have multiple matches and clicking the main Fetch button, add all of them
                if (this.nodes.get().length > 0) {
                    this.fetchConfirmId = 'MULTIPLE'; // Marker for multiple fetch
                    this.showFetchConfirm = true;
                } else {
                    this.fetchMultipleHierarchies(this.fetchResults);
                }
            } else if (query) {
                if (this.nodes.get().length > 0) {
                    this.fetchConfirmId = query;
                    this.showFetchConfirm = true;
                } else {
                    this.fetchHierarchy(query);
                }
            }
        },
        async handleFetchChoice(choice) {
            this.showFetchConfirm = false;
            const targetId = this.fetchConfirmId;
            this.fetchConfirmId = null;

            if (choice === 'overwrite') {
                await this.clearCanvas(true);
                if (targetId === 'MULTIPLE') {
                    this.fetchMultipleHierarchies(this.fetchResults);
                } else {
                    this.fetchHierarchy(targetId);
                }
            } else if (choice === 'add') {
                if (targetId === 'MULTIPLE') {
                    this.fetchMultipleHierarchies(this.fetchResults);
                } else {
                    this.fetchHierarchy(targetId);
                }
            }
        },
        addExistingEntity(item) {
            if (!item) return;
            const targetId = item.Id || item.CIF || item.customerId || item.onb_cust_id;
            const nodes = Alpine.raw(this.nodes);

            this.fetchResults = [];
            this.fetchQuery = '';
            this.isHistoryMode = true;

            if (nodes.get().length > 0) {
                this.fetchConfirmId = targetId;
                this.showFetchConfirm = true;
            } else {
                this.fetchHierarchy(targetId);
            }
        },
        async fetchMultipleHierarchies(items) {
            if (!items || items.length === 0) return;
            toastr.info(`Fetching hierarchies for ${items.length} entities...`);
            for (const item of items) {
                const id = item.Id || item.CIF || item.customerId || item.onb_cust_id;
                if (id) await this.fetchHierarchy(id, true);
            }
        },
        async fetchHierarchy(id, silent = false, isDuplicateImport = false) {
            if (!id) return;

            if (!silent) {
                this.fetchResults = [];
                this.fetchQuery = ''; // Clear query on start

                Swal.fire({
                    title: 'Loading Hierarchy...',
                    text: 'Please wait while we fetch and map relationships',
                    allowOutsideClick: false,
                    didOpen: () => Swal.showLoading()
                });
            }

            try {
                const r = await fetch(`/case/studio/fetch-hierarchy?id=${encodeURIComponent(id)}`);
                if (r.status === 404) {
                    if (!silent) toastr.info('No direct hierarchy found. Try searching for the name instead.');
                    if (!silent) Swal.close();
                    return;
                }
                if (!r.ok) throw new Error('Fetch failed');
                const result = await r.json();
                const entities = result.entities || result; // Fallback for old API if needed
                const rootAttachments = result.attachments || [];

                if (entities && entities.length > 0) {
                    const network = Alpine.raw(this.network);
                    const nodes = Alpine.raw(this.nodes);
                    const edges = Alpine.raw(this.edges);

                    let viewCenter = { x: 0, y: 0 };
                    try {
                        if (network) viewCenter = network.getViewPosition();
                    } catch (e) { }

                    // Temporarily disable hierarchical layout to avoid layout engine crashes during bulk add
                    if (network) network.setOptions({ layout: { hierarchical: { enabled: false } } });

                    // 1. Add all nodes
                    entities.forEach((entity, index) => {
                        const nodeId = (entity.customerId || entity.CustomerId || entity.id || entity.Id || entity.cifNumber || entity.CIFNumber || `TEMP_${index}`).toString();

                        if (!nodes.get(nodeId)) {
                            // Robust label detection
                            const firstName = entity.firstName || entity.FirstName || '';
                            const lastName = entity.lastName || entity.LastName || '';
                            const fullName = entity.fullName || entity.FullName || (firstName + ' ' + lastName).trim();
                            const finalLabel = fullName || entity.cifNumber || entity.CIFNumber || nodeId;

                            const type = this.normalizeType(entity.customerType || entity.CustomerType || entity.type || entity.Type || 'I');
                            const parentId = entity.parentID || entity.ParentID || entity.companyCode || entity.CompanyCode;
                            const hasParentLink = parentId && parentId.toString() !== "0" && parentId.toString() !== "null" && parentId.toString().trim() !== "";
                            // Automatically designate the searched entity (index 0) as root if it has no parent link, 
                            // OR if it's the first node being added to an empty canvas.
                            const isRootNode = (index === 0 && nodes.get().length === 0) || !hasParentLink;
                            const caseId = entity.customercode || entity.customerCode || entity.companyCode || entity.CompanyCode || entity.customerId || entity.CustomerId || entity.onb_cust_ref_id || entity.onb_customerid || entity.caseId || entity.CaseId || entity.id || entity.Id || '';
                            const gidRaw = entity.groupId || entity.GroupId || '';
                            const groupIds = gidRaw ? gidRaw.split(',').map(g => g.trim()).filter(Boolean) : [];
                            const isMain = !!(isRootNode || (isDuplicateImport ? (entity.isMainEntity || entity.IsMainEntity) : (entity.type === 'Corporate' || entity.type === 'Individual' || entity.Type === 'Corporate' || entity.Type === 'Individual' || entity.isMainEntity || entity.IsMainEntity)));

                            nodes.add({
                                ...entity,
                                id: nodeId,
                                caseId: caseId,
                                databaseId: isDuplicateImport ? 0 : (entity.id || entity.Id || entity.customerId || entity.CustomerId || ''),
                                cif: entity.customerId || entity.Id || entity.cifNumber || entity.CIFNumber || '', // Preserve original entity ID for backend linkage
                                label: finalLabel,
                                attachments: (index === 0) ? rootAttachments : [], // Root gets attachments
                                x: viewCenter.x + (index * 50),
                                y: viewCenter.y + (index * 50),
                                image: this.getSvg(type, isRootNode, true, !!(entity.isDuplicate || entity.IsDuplicate), caseId, undefined, isMain),
                                type: type,
                                isRoot: isRootNode,
                                isScreened: true,
                                isFetched: isDuplicateImport ? false : true,
                                isMainEntity: isMain,
                                isDuplicate: isDuplicateImport ? true : !!(entity.isDuplicate || entity.IsDuplicate),
                                groupId: groupIds, // Store as array
                                groupRisk: entity.groupRisk || entity.GroupRisk || 'Unclassified',
                                name: finalLabel,
                                level: (this.activeLayout === 'hierarchical') ? (entity.level !== undefined ? entity.level : 0) : null,
                                shareholderType: (entity.Type || entity.type || '')
                            });
                        }
                    });

                    // 3. Sync Group Definitions from Nodes
                    nodes.get().forEach(n => {
                        const gids = Array.isArray(n.groupId) ? n.groupId : (n.groupId ? [n.groupId] : []);
                        gids.forEach(gid => {
                            if (!gid) return;
                            let group = this.groups.find(g => g.id === gid);
                            if (!group) {
                                // Assign a simple Gx label instead of the long GUID string
                                const groupIndex = this.groups.length + 1;
                                group = {
                                    id: gid,
                                    nodeIds: [],
                                    label: `Group G${groupIndex}`,
                                    risk: n.groupRisk || 'Unclassified'
                                };
                                this.groups.push(group);
                            }
                            if (!group.nodeIds.includes(n.id)) {
                                group.nodeIds.push(n.id);
                            }
                        });
                    });

                    // Add the root node to history
                    if (entities[0]) this.addToHistory(entities[0]);

                    // 2. Add edges (handle multiple parents in comma-separated ParentID)
                    entities.forEach(entity => {
                        const nodeId = (entity.customerId || entity.CustomerId || entity.id || entity.Id || entity.cifNumber || entity.CIFNumber);
                        if (!nodeId) return;

                        const sNodeId = nodeId.toString();
                        const parentIdField = entity.parentID || entity.ParentID || entity.companyCode || entity.CompanyCode;

                        if (!parentIdField) return;

                        // Split comma-separated parent IDs to support multiple parent relationships (e.g., ISH of Entity1 + AS of Entity2)
                        const parentIds = parentIdField.toString()
                            .split(',')
                            .map(p => p.trim())
                            .filter(p => p && p !== "0" && p !== sNodeId);

                        const rawRelFull = entity.relationship || entity.Relationship || entity.flagType || entity.FlagType || '';
                        const rawRels = rawRelFull.toString().split(',').map(r => r.trim());

                        parentIds.forEach((parentId, index) => {
                            if (nodes.get(parentId) && nodes.get(sNodeId)) {
                                const edgeId = `edge-${parentId}-${sNodeId}`;
                                if (!edges.get(edgeId)) {
                                    const rawRel = rawRels[index] || '';
                                    let relEnum = this.getRelationshipEnum(rawRel);
                                    if (!relEnum) {
                                        const eType = entity.customerType || entity.type || entity.CustomerType || entity.Type;
                                        relEnum = (eType === 'C' || eType === 'Corporate') ? 'corporate_shareholder' : 'individual_shareholder';
                                    }
                                    edges.add({
                                        id: edgeId,
                                        from: parentId,
                                        to: sNodeId,
                                        relationship: relEnum,
                                        label: this.getRelationshipShortform(relEnum) || rawRel
                                    });
                                }
                            }
                        });
                    });

                    this.autoLayout('hierarchical', true);
                    this.fetchQuery = '';
                    toastr.success('Workspace updated with fetched hierarchy.', 'Success');
                } else {
                    toastr.info('No data returned for this hierarchy.', 'Info');
                }
                Swal.close();
            } catch (e) {
                console.error(e);
                Swal.fire('Error', 'Could not load hierarchy for this ID.', 'error');
            }
        },
        handleOCRUpload(e) {
            const file = e.target.files[0];
            if (!file) return;

            toastr.info('AI is analyzing the document...', 'OCR Intelligence');

            // Simulation of OCR Extraction
            setTimeout(() => {
                const type = this.formData.type;
                if (type === 'C') {
                    this.formData.ocrResults = [
                        { id: 'ocr_c1', name: 'Al Ghurair Group', type: 'C', confidence: 99 },
                        { id: 'ocr_c2', name: 'Abdulla Al Ghurair', type: 'I', confidence: 96 },
                        { id: 'ocr_c3', name: 'Majid Al Ghurair', type: 'I', confidence: 92 }
                    ];
                } else {
                    this.formData.ocrResults = [
                        { id: 'ocr_i1', name: 'Mohammed Al Hashimi', type: 'I', confidence: 98 },
                        { id: 'ocr_i2', name: 'Fatima Al Hashimi (Spouse)', type: 'I', confidence: 91 }
                    ];
                }
                toastr.success('Document analysis complete.', 'Extraction Finished');
                this.$nextTick(() => lucide.createIcons());
            }, 2500);
        },
        addOCRNode(item) {
            const id = 'node-' + Math.random().toString(36).substr(2, 9);
            const network = Alpine.raw(this.network);
            const viewCenter = network.getViewPosition();

            this.nodes.add({
                id: id,
                label: item.name,
                type: item.type,
                level: (this.activeLayout === 'hierarchical') ? 0 : null,
                x: viewCenter.x + (Math.random() * 200 - 100),
                y: viewCenter.y + (Math.random() * 200 - 100)
            });

            toastr.success(`Imported ${item.name} to workspace.`);
            this.formData.ocrResults = this.formData.ocrResults.filter(r => r.id !== item.id);
        },
        addAllOCR() {
            this.formData.ocrResults.forEach(item => {
                const id = 'node-' + Math.random().toString(36).substr(2, 9);
                this.nodes.add({
                    id: id,
                    label: item.name,
                    type: item.type,
                    level: (this.activeLayout === 'hierarchical') ? 0 : null,
                    x: (Math.random() * 400 - 200),
                    y: (Math.random() * 400 - 200)
                });
            });
            toastr.success(`Imported ${this.formData.ocrResults.length} entities.`);
            this.formData.ocrResults = [];
            this.closeSheet();
        },
        handleBulkFileUpload(e) {
            const file = e.target.files[0];
            if (!file) return;

            toastr.info('Processing entity dataset...', 'Bulk Import');

            setTimeout(() => {
                const rootId = 'bulk-root-' + Date.now();
                const network = Alpine.raw(this.network);
                const center = network.getViewPosition();

                // Add Root
                this.nodes.add({
                    id: rootId,
                    label: 'Imported Group Entity',
                    type: 'C',
                    level: (this.activeLayout === 'hierarchical') ? 0 : null,
                    x: center.x,
                    y: center.y
                });

                // Add Children
                for (let i = 1; i <= 3; i++) {
                    const childId = `bulk-child-${i}-${Date.now()}`;
                    this.nodes.add({
                        id: childId,
                        label: `Stakeholder ${i}`,
                        type: i % 2 === 0 ? 'C' : 'I',
                        level: (this.activeLayout === 'hierarchical') ? 1 : null,
                        x: center.x + (i * 150),
                        y: center.y + 150
                    });

                    this.edges.add({
                        id: `edge-${rootId}-${childId}`,
                        from: rootId,
                        to: childId,
                        label: ''
                    });
                }

                toastr.success('Bulk import successful. 4 nodes created.', 'Success');
                this.autoLayout('hierarchical', true);
                this.closeSheet();
            }, 3000);
        },
        openBulkUpload() {
            this.formData = this.resetFormData();
            this.formData.isBulkMode = true;
            this.activeTab = 'bulk';
            this.showSheet = true;
        },
        openOCR() {
            this.formData = this.resetFormData();
            this.formData.isOCRMode = true;
            this.formData.type = 'I';
            this.activeTab = 'identity';
            this.showSheet = true;
        },
        toggleAllScreening(e) {
            if (e.target.checked) {
                this.formData.screeningSources = window.studioData.ScreeningOptions.map(o => o.Text || o.text);
            } else {
                this.formData.screeningSources = [];
            }
        },
        startConnection(sourceId) {
            this.setTool('connect'); // Ensure we are in link mode to prevent opening sheet
            const network = Alpine.raw(this.network);
            const pos = network.getPositions([sourceId])[sourceId];
            const domPos = network.canvasToDOM(pos);

            this.linkSourceId = sourceId;
            this.linkSourcePos = { x: domPos.x, y: domPos.y };
            this.linkMousePos = { ...this.linkSourcePos };
            this.isLinking = true;
            this.hoveredNodeId = null;
            // The setTool already shows a toast, so we can omit the second one or make it specific
        },
        cancelLink() {
            this.isLinking = false;
            this.linkSourceId = null;
            this.setTool('pointer'); // Revert to pointer mode after link
        },
        checkDuplicates(nodeId) {
            this.searchNodeId = nodeId;
            const node = this.nodes.get(nodeId);
            if (!node) return;

            // Map studio types to legacy backend types
            let type = 'I';
            if (node.type === 'C' || node.type === 'Corporate') {
                type = 'C';
            }

            // Ensure we have a name to check
            const name = this.formData.name || node.label || '';
            if (!name) {
                toastr.warning('Please enter a name first.');
                return;
            }

            toastr.info(`Checking duplicates for ${name}...`, 'System Check');

            fetch(`/case/Checkduplicatenames?fullname=${encodeURIComponent(name)}&type=${type}`)
                .then(response => response.json())
                .then(data => {
                    console.log('Duplicate Check Results:', data);
                    this.duplicateResults = data || [];
                    this.isDuplicateModalOpen = true;
                    this.$nextTick(() => {
                        if (typeof lucide !== 'undefined') lucide.createIcons();
                    });
                    if (this.duplicateResults.length === 0) {
                        toastr.success('Database check complete: No existing records found.', 'System Check');
                    } else {
                        toastr.warning(`${this.duplicateResults.length} potential matches identified in the system.`, 'Security Alert');
                    }
                })
                .catch(error => {
                    console.error('Error checking duplicates:', error);
                    toastr.error('Failed to check duplicates.');
                });
        },
        closeDuplicateModal() {
            this.isDuplicateModalOpen = false;
            this.duplicateResults = [];
        },
        processDuplicate(item) {
            if (!item || !item.customerId) return;
            this.duplicateToProcess = item;
            this.isProcessActionModalOpen = true;
        },
        isTypeMatch(targetType) {
            if (!this.duplicateToProcess) return false;
            // Use same normalization as the rest of the system
            const rawType = this.duplicateToProcess.type || this.duplicateToProcess.customerType || this.duplicateToProcess.matchCategory || '';
            return this.normalizeType(rawType) === targetType;
        },
        async importDuplicate(roleId, isRoot) {
            if (!this.duplicateToProcess) return;
            const item = this.duplicateToProcess;
            if (isRoot) {
                await this.importAsMainParty(item);
            } else {
                await this.selectDuplicate(item, roleId);
            }
        },
        async importAsMainParty(item = null) {
            const duplicate = item || this.duplicateToProcess;
            if (!duplicate) return;

            this.isProcessActionModalOpen = false;
            this.closeDuplicateModal();

            Swal.fire({
                title: 'Importing Main Entity',
                text: 'Replacing current context with system record...',
                didOpen: () => Swal.showLoading()
            });

            try {
                // Use fetchHierarchy logic instead of shallow fetch-party
                toastr.info('Fetching full hierarchy for system record...');

                // We clear current context if it's the main party import
                // But fetchHierarchy adds nodes. So we should identify the current root and remove it.
                const nodes = Alpine.raw(this.nodes);
                const currentRoot = nodes.get({ filter: n => n.isRoot })[0];
                if (currentRoot) {
                    this.deleteNode(currentRoot.id);
                }

                await this.fetchHierarchy(duplicate.customerId, true, true);

                Swal.fire({
                    icon: 'success',
                    title: 'Imported',
                    text: 'Main entity and its relationships have been synchronized.',
                    timer: 2000
                });
                this.autoLayout('hierarchical', true);
                this.updateTree();
                Swal.close();
            } catch (err) {
                console.error(err);
                Swal.fire('Error', 'Failed to import full hierarchy: ' + err.message, 'error');
            }
        },
        async selectDuplicate(item, roleId = null) {
            if (!item || !item.customerId) return;

            const nodes = Alpine.raw(this.nodes);
            if (nodes.get().length === 0) {
                toastr.warning('Please add a Main Entity before adding shareholders or related parties.', 'Action Blocked');
                return;
            }

            this.isProcessActionModalOpen = false;
            this.closeDuplicateModal();
            Swal.fire({
                title: 'Fetching Data...',
                text: 'Hydrating entity from system records',
                didOpen: () => Swal.showLoading(),
                customClass: {
                    popup: 'rounded-3xl border-none shadow-2xl p-10',
                    title: 'text-lg font-black text-slate-800'
                }
            });

            try {
                const r = await fetch(`/case/studio/fetch-party?id=${item.customerId}`);
                if (!r.ok) throw new Error(`Server returned ${r.status}`);
                const result = await r.json();

                if (result && result.entity) {
                    const data = result.entity;
                    const attachments = result.attachments || [];

                    const network = Alpine.raw(this.network);
                    const nodes = Alpine.raw(this.nodes);

                    if (network) network.setOptions({ layout: { hierarchical: { enabled: false } } });

                    const viewCenter = network.getViewPosition();
                    const shortId = Math.random().toString(36).substring(2, 6).toUpperCase();
                    const nodeId = 'S_' + Date.now() + '_' + shortId;

                    const caseId = data.customercode || data.customerCode || data.companyCode || data.CompanyCode || data.customerId || data.CustomerId || data.onb_cust_ref_id || data.onb_customerid || data.caseId || data.CaseId || data.id || data.Id || '';
                    const finalName = data.name || data.Name || [data.firstName, data.middleName, data.lastName].filter(Boolean).join(' ').trim() || data.label || 'Unnamed';
                    const type = this.normalizeType(data.customerType || data.Type || (data.matchCategory?.toUpperCase().startsWith('CORP') ? 'C' : 'I'));
                    // Check if there is an orphan node to replace
                    let orphanNode = null;
                    if (this.searchNodeId) {
                        orphanNode = nodes.get(this.searchNodeId);
                    }

                    const isRoot = orphanNode ? !!orphanNode.isRoot : false;

                    const newNode = {
                        ...data,
                        id: nodeId,
                        caseId: caseId,
                        metadata: data, // Explicit metadata for openNode mapping
                        attachments: attachments, // Attachments from backend
                        isFetched: false, // Set to false to trigger new case creation for duplicates
                        isMainEntity: orphanNode ? !!orphanNode.isMainEntity : isRoot,
                        isScreened: true,
                        label: finalName,
                        x: orphanNode ? orphanNode.x : (viewCenter.x + 20),
                        y: orphanNode ? orphanNode.y : (viewCenter.y + 20),
                        image: this.getSvg(type, isRoot, true, true, caseId, undefined, orphanNode ? !!orphanNode.isMainEntity : isRoot),
                        type: type,
                        isRoot: isRoot,
                        relationship: roleId || (orphanNode ? orphanNode.relationship : ''),
                        shareholderType: type === 'C' ? 'Corporate' : 'Individual',
                        shortId: shortId,
                        databaseId: 0,
                        isDuplicate: true,
                        cif: data.customerId || data.Id || data.cif || '', // Preserve original entity ID for backend linkage
                        level: (this.activeLayout === 'hierarchical') ? (orphanNode ? orphanNode.level : 0) : null
                    };

                    nodes.add(newNode);

                    // Link parents and children if replacing an orphan node
                    if (orphanNode && this.searchNodeId) {
                        const edges = Alpine.raw(this.edges);
                        const originalIdStr = this.searchNodeId.toString();
                        const incomingEdges = edges.get().filter(e => e.to.toString() === originalIdStr);
                        const outgoingEdges = edges.get().filter(e => e.from.toString() === originalIdStr);

                        // Remove old edges
                        edges.remove(incomingEdges.map(e => e.id));
                        edges.remove(outgoingEdges.map(e => e.id));

                        // Add new edges pointing to/from the new duplicate node
                        incomingEdges.forEach(e => {
                            edges.add({
                                ...e,
                                id: `edge-${e.from}-${nodeId}`,
                                to: nodeId
                            });
                        });

                        outgoingEdges.forEach(e => {
                            edges.add({
                                ...e,
                                id: `edge-${nodeId}-${e.to}`,
                                from: nodeId
                            });
                        });

                        // Remove the orphan node
                        nodes.remove(this.searchNodeId);
                        this.searchNodeId = null;
                    }

                    this.updateHierarchyTypes(nodeId);
                    this.autoLayout('hierarchical', true);
                    this.centerNode(nodeId);

                    // Open updated node data to ensure classification badges and relationship labels are correct
                    this.openNode(nodes.get(nodeId));
                    this.updateTree();

                    Swal.close();
                    toastr.success('New entity node added from system records.', 'System Sync');
                    this.$nextTick(() => { if (typeof lucide !== 'undefined') lucide.createIcons(); });
                }
            } catch (e) {
                console.error(e);
                Swal.fire('Fetch Error', 'Failed to retrieve entity data.', 'error');
            }
        },
        onDragStart(e, type, isRoot) {
            e.dataTransfer.setData('nodeType', type);
            e.dataTransfer.setData('isRoot', isRoot);
        },
        handleDrop(e) {
            const type = e.dataTransfer.getData('nodeType');
            const isRoot = e.dataTransfer.getData('isRoot') === 'true';
            const rect = document.getElementById('studio-canvas').getBoundingClientRect();
            const network = Alpine.raw(this.network);
            const canvasPosition = network.DOMtoCanvas({ x: e.clientX - rect.left, y: e.clientY - rect.top });
            this.addNode(type, isRoot, canvasPosition.x, canvasPosition.y);
        },
        addNode(type, isRoot, x, y, roleId = null) {
            const network = Alpine.raw(this.network);
            const nodes = Alpine.raw(this.nodes);

            if (!isRoot && nodes.get().length === 0) {
                toastr.warning('Please add a Main Entity before adding shareholders or related parties.', 'Action Blocked');
                return null;
            }

            network.setOptions({ layout: { hierarchical: { enabled: false } } });
            const shortId = Math.random().toString(36).substring(2, 6).toUpperCase();
            const id = 'S_' + Date.now() + '_' + shortId;

            const roleLabel = roleId ? this.getRelationshipLabel(roleId) : (isRoot ? 'Main Entity' : 'Related Party');
            const label = roleLabel + ' ' + shortId;

            const shType = type === 'C' ? 'Corporate' : 'Individual';

            nodes.add({
                id, label, x, y,
                image: this.getSvg(type, isRoot, false, false, undefined, undefined, !!isRoot),
                type, isRoot, isScreened: false,
                isFetched: false,
                isMainEntity: !!isRoot,
                relationship: roleId || (isRoot ? 'root' : ''),
                shareholderType: shType,
                shortId: shortId,
                level: (this.activeLayout === 'hierarchical') ? 0 : null
            });

            this.updateHierarchyTypes(id);

            // Select and open the new node
            this.$nextTick(() => {
                this.centerNode(id);
                if (typeof lucide !== 'undefined') lucide.createIcons();
            });

            return id;
        },
        clickAdd(type, isRoot, roleId = null) {
            const network = Alpine.raw(this.network);
            network.setOptions({ layout: { hierarchical: { enabled: false } } });

            const viewCenter = network.getViewPosition();
            // Offset slightly from center if center is occupied
            const newId = this.addNode(type, isRoot, viewCenter.x, viewCenter.y, roleId);

            if (roleId) {
                this.setRelatedPartyRole(roleId);
            }

            if (!isRoot && this.selectedNodes.length === 1 && this.selectedNodes[0] !== newId) {
                this.createEdge(this.selectedNodes[0], newId);
                this.updateTree();
                toastr.success('Related party linked to selection.', 'Auto-Link');
            }
        },
        promoteNode(nodeId) {
            const nodes = Alpine.raw(this.nodes);
            const targetNode = nodes.get(nodeId);
            if (!targetNode || targetNode.isRoot) return;

            const currentRoot = nodes.get().find(n => n.isRoot);
            if (currentRoot) {
                nodes.update({
                    id: currentRoot.id,
                    isRoot: false,
                    level: (this.activeLayout === 'hierarchical') ? 1 : null,
                    image: this.getSvg(currentRoot.type || (currentRoot.customerType === 'C' ? 'C' : 'I'), false, currentRoot.isScreened, currentRoot.isDuplicate, currentRoot.caseId, undefined, !!(currentRoot.isMainEntity || currentRoot.isRoot))
                });
            }

            nodes.update({
                id: nodeId,
                isRoot: true,
                isMainEntity: true,
                level: (this.activeLayout === 'hierarchical') ? 0 : null,
                image: this.getSvg(targetNode.type || (targetNode.customerType === 'C' ? 'C' : 'I'), true, targetNode.isScreened, targetNode.isDuplicate, targetNode.caseId, undefined, true)
            });

            if (this.selectedNode && this.selectedNode.id === nodeId) {
                this.selectedNode.isMainEntity = true;
                this.formData.isMainEntity = true;
            }

            this.updateTree();
            this.autoLayout('hierarchical', true);
            toastr.success('Node promoted to Main Entity.', 'Hierarchy Updated');
        },
        centerNode(nodeId, event) {
            const network = Alpine.raw(this.network);
            const nodes = Alpine.raw(this.nodes);
            if (!network || !nodeId) return;
            const isMulti = event && (event.shiftKey || event.ctrlKey || event.metaKey);
            const strId = nodeId.toString();

            if (isMulti) {
                const currentSelection = network.getSelectedNodes().map(id => id.toString());
                if (currentSelection.includes(strId)) {
                    network.selectNodes(currentSelection.filter(id => id !== strId));
                } else {
                    network.selectNodes([...currentSelection, strId]);
                }
            } else {
                network.selectNodes([nodeId]);
                network.focus(nodeId, {
                    scale: 1,
                    animation: { duration: 800, easingFunction: 'easeInOutQuad' }
                });
                const node = nodes.get(nodeId);
                if (node) this.openNode(node);
            }
            this.selectedNodes = network.getSelectedNodes().map(id => id.toString());
            this.updateBoundingBox();
        },
        openNode(node) {
            this.selectedNode = node;
            this.isReadOnly = !!node.isFetched;

            // Extract data from node (standard) or node.metadata (fetched)
            const data = node.metadata || node;
            console.log('Opening Node Data:', { node, data });

            // Create a clean starting point
            const base = this.resetFormData();

            // Check if node has a parent in current canvas structure
            const edges = Alpine.raw(this.edges);
            const hasParent = edges && edges.get({ filter: e => e.to.toString() === node.id.toString() }).length > 0;

            // Comprehensive Mapping (Supports both camelCase from Studio and PascalCase from System Records)
            base.id = node.id;
            base.caseId = data.customercode || data.customerCode || data.companyCode || data.CompanyCode || data.customerId || data.CustomerId || data.onb_cust_ref_id || data.onb_customerid || data.caseId || data.CaseId || node.caseId || (node.databaseId && node.databaseId !== '0' && node.databaseId !== 0 ? node.databaseId : '') || data.id || data.Id || '';
            base.isRoot = !!node.isRoot;
            base.isMainEntity = !!node.isMainEntity;
            base.isScreened = !!node.isScreened;
            base.isDuplicate = !!node.isDuplicate;
            base.databaseId = node.databaseId || '';

            // Name mapping with all possible fallbacks
            let name = node.name || data.name || data.Name || data.firstName || data.FirstName || data.lastName || data.LastName || data.companyName || data.CompanyName || data.fullName || data.FullName || data.onb_name || '';

            // If name is empty and label contains default text, show placeholder instead
            if (!name && node.label) {
                const label = node.label.split('\n')[0];
                // Detect fallback patterns like "Main Entity M740", "Authorized Signatory A1B2", etc.
                if (label.includes('Main Entity') || label.includes('Corporate') || label.includes('Individual') ||
                    label.includes('Related Party') || label.includes('Shareholder') || label.includes('Main Party') ||
                    label.includes('Authorized Signatory') || label.includes('Authorised Signatory') ||
                    label.includes('Senior Management')) {
                    name = ''; // Keep empty to show placeholder
                } else {
                    name = label;
                }
            }

            base.name = name;
            const fName = data.firstName || data.FirstName;
            const lName = data.lastName || data.LastName;
            if (fName && lName && (!base.name || base.name === fName)) {
                base.name = `${fName} ${lName}`.trim();
            }

            // Normalizing type using robust helper
            base.type = this.normalizeType(data.customerType || data.CustomerType || data.type || data.Type || data.onb_cust_type || (data.matchCategory?.toUpperCase().startsWith('CORP') ? 'C' : 'I'));

            base.nationality = data.nationality || data.Nationality || data.onb_nationality || data.nationalityName || data.NationalityName || '';
            base.dob = this.formatDateForInput(data.dob || data.DOB || data.onb_dob || data.custDOB || data.establishmentDate || data.EstablishmentDate);
            base.gender = data.gender || data.Gender || '';
            base.cif = data.cifNumber || data.CIFNumber || '';
            
            const rawRel = node.relationship || data.relationship || data.Relationship || data.flagType || data.FlagType || '';
            let relMapped = this.getRelationshipEnum(rawRel) || (node.parentID ? 'child' : 'root');
            if (hasParent && (relMapped === 'root' || relMapped === 'child')) {
                relMapped = node.type === 'C' ? 'corporate_shareholder' : 'individual_shareholder';
            }
            base.relationship = relMapped;

            // ID Documents
            base.idType = data.customerIdType || data.onb_cust_id_type || data.idType || data.IdType || '';
            base.idNumber = data.customerIdNumber || data.onb_cust_id || data.idNumber || data.IdNumber || '';

            base.passportId = data.passportId || data.PassportId || data.onb_passport_no || '';
            base.passportIssueDate = this.formatDateForInput(data.passportIssueDate || data.PassportIssueDate || data.onb_passport_issue || data.IdIssueDate);
            base.passportExpiryDate = this.formatDateForInput(data.passportExpiryDate || data.PassportExpiryDate || data.onb_passport_expiry || data.IdExpiryDate);

            base.emiratesIdNumber = data.emiratesIdNumber || data.EmiratesIdNumber || data.onb_emirates_id || '';
            base.emiratesIdIssueDate = this.formatDateForInput(data.emiratesIdIssueDate || data.EmiratesIdIssueDate || data.onb_emirates_id_issue);
            base.emiratesIdExpiryDate = this.formatDateForInput(data.emiratesIdExpiryDate || data.EmiratesIdExpiryDate || data.onb_emirates_id_expiry);

            // Profile
            base.profession = data.profession || data.Profession || data.onb_profession || data.occupatinTypeTxt || data.OccupatinTypeTxt || '';
            base.residence = data.residence || data.Residence || data.onb_residence_status || data.residenceStatus || data.ResidenceStatus || '';
            base.placeOfBirth = data.placeOfBirth || data.PlaceOfBirth || '';
            base.sowsofcountry = data.sowsofCountry || data.SOWSOFCountry || data.sowsofcountry || '';

            // Corporate Specific
            base.tradeLicence = data.tradeLicence || data.TradeLicence || data.tradelicense || data.Tradelicense || data.companyCode || data.CompanyCode || '';
            base.tradeLicenseAuthority = data.tradeLicenseAuthority || data.TradeLicenseAuthority || '';
            base.registrationDate = this.formatDateForInput(data.registrationDate || data.RegistrationDate || data.establishmentDate || data.EstablishmentDate || data.onb_created_on);
            base.entityTypeTxt = data.entityTypeTxt || data.EntityTypeTxt || '';
            base.businessType = data.businessType || data.BusinessType || '';

            // Onboarding Profile
            base.counterParty = data.counterParty || data.CounterParty || '';
            base.counterPartyName = data.counterPartyName || data.CounterPartyName || '';
            base.productType = data.productName || data.ProductName || data.productType || data.ProductType || data.onb_insurance_product || '';
            base.productValue = data.productValue || data.ProductValue || '';
            base.deliveryChannel = data.deliveryChannelName || data.DeliveryChannelName || data.deliveryChannel || data.DeliveryChannel || data.onb_delv_channel || '';
            base.paymentMode = data.modeofpayment || data.Modeofpayment || data.paymentMode || data.PaymentMode || data.onb_mode_of_pymt || '';

            // Employer
            base.employer = data.employer || data.Employer || '';
            base.employerIndustry = data.employerIndustry || data.EmployerIndustry || '';
            base.employerSector = data.employerSector || data.EmployerSector || '';
            base.goldenVisa = data.goldenVisa || data.GoldenVisa || 'No';
            base.productRefNo = data.productRefNo || data.ProductRefNo || '';
            base.shareholderType = data.shareholderType || data.ShareholderType || data.Type || '';

            // Meta & Scoring
            base.matchParameter = data.matchParameter || data.threshold || data.Threshold || 85;
            base.matchScore = data.matchScore || data.CustomerScreenMatchScore || 0;
            base.selectedIdTypes = base.idType ? base.idType.split(',').map(s => s.trim()).filter(s => s) : [];

            // Additional State Preservation
            base.attachments = data.attachments || data.Attachments || [];
            base.riskClass = data.riskClass || '';
            base.riskRating = data.riskRating || '';
            base.screeningSources = data.screeningSources || (data.screeningOption ? [data.screeningOption] : ['1']);
            base.share = data.share || 0;
            base.designation = data.designation || '';
            const rawGid = data.groupId || node.groupId || [];
            base.groupId = Array.isArray(rawGid) ? rawGid : (rawGid ? [rawGid] : []);
            const rawRisk = data.groupRisk || node.groupRisk || [];
            base.groupRisk = Array.isArray(rawRisk) ? rawRisk : (rawRisk ? [rawRisk] : []);

            this.formData = base;
            this.activeTab = 'identity';
            this.showSheet = true;
            this.$nextTick(() => {
                if (window.lucide) lucide.createIcons();
                // Initialize all Flatpickr instances
                if (window.flatpickr) {
                    const pickers = [
                        { id: "#dob-picker", field: 'dob' },
                        { id: "#reg-date-picker", field: 'registrationDate' },
                        { id: "#p-issue-picker", field: 'passportIssueDate' },
                        { id: "#p-expiry-picker", field: 'passportExpiryDate' },
                        { id: "#e-issue-picker", field: 'emiratesIdIssueDate' },
                        { id: "#e-expiry-picker", field: 'emiratesIdExpiryDate' },
                        // New Role-Specific Pickers
                        { id: "#dob-picker-main", field: 'dob' },
                        { id: "#reg-date-picker-main", field: 'registrationDate' },
                        { id: "#exp-date-picker-main", field: 'expiryDate' },
                        { id: "#dob-picker-related", field: 'dob' },
                        { id: "#reg-date-picker-corp", field: 'registrationDate' },
                        { id: "#expiry-date-picker-corp", field: 'expiryDate' },
                        { id: "#dob-picker-sh", field: 'dob' },
                        { id: "#reg-date-picker-sh-corp", field: 'registrationDate' },
                        { id: "#expiry-date-picker-sh-corp", field: 'expiryDate' },
                        { id: "#dob-picker-as", field: 'dob' },
                        { id: "#dob-picker-sm", field: 'dob' }
                    ];

                    pickers.forEach(p => {
                        const el = document.querySelector(p.id);
                        if (el) {
                            flatpickr(el, {
                                dateFormat: "d/m/Y",
                                allowInput: true,
                                defaultDate: base[p.field] || null,
                                onChange: (selectedDates, dateStr) => {
                                    this.formData[p.field] = dateStr;
                                }
                            });
                        }
                    });
                }

                // Reset scroll position
                const sheetContent = document.querySelector('#studio-sheet .overflow-y-auto');
                if (sheetContent) sheetContent.scrollTop = 0;
                if (window.lucide) lucide.createIcons();
            });
        },
        updateNode() {
            if (this.isReadOnly) return;
            if (!this.validateForm()) return;
            const updated = {
                ...this.selectedNode,
                ...this.formData,
                label: (this.formData.name || this.selectedNode.label.split('\n')[0]),
                image: this.getSvg(this.formData.type, this.formData.isRoot, this.formData.isScreened, this.formData.isDuplicate, this.formData.caseId, undefined, !!(this.selectedNode.isMainEntity || this.selectedNode.isRoot))
            };
            const updatePayload = {
                id: this.selectedNode.id,
                label: updated.label,
                image: updated.image,
                caseId: this.formData.caseId,
                ...this.formData
            };
            if (this.selectedNode.metadata) {
                this.selectedNode.metadata = {
                    ...this.selectedNode.metadata,
                    ...this.formData
                };
                updatePayload.metadata = this.selectedNode.metadata;
            }
            this.nodes.update(updatePayload);
            this.updateHierarchyTypes(this.selectedNode.id);
            const edges = Alpine.raw(this.edges);
            const incomingEdges = edges.get({
                filter: (edge) => edge.to === this.selectedNode.id
            });
            incomingEdges.forEach(edge => {
                edges.update({
                    id: edge.id,
                    label: '' // Maintain clean canvas
                });
            });

            this.closeSheet();
        },
        quickAdd(type, parentId) {
            if (!parentId) return;
            const network = Alpine.raw(this.network);
            const nodes = Alpine.raw(this.nodes);
            network.setOptions({ layout: { hierarchical: { enabled: false } } });
            const parentPos = network.getPositions([parentId])[parentId];
            const shortId = Math.random().toString(36).substring(2, 6).toUpperCase();
            const id = 'S_' + Date.now() + '_' + shortId;

            // Set default relationship for quickAdd (Shareholder)
            const roleId = type === 'C' ? 'corporate_shareholder' : 'individual_shareholder';
            const shType = type === 'C' ? 'Corporate' : 'Individual';

            const finalLabel = (type === 'C' ? 'Corporate' : 'Individual') + ' Shareholder ' + shortId;
            const parentNode = nodes.get(parentId);
            const parentLevel = parentNode ? parentNode.level : null;
            const calculatedLevel = (parentLevel !== undefined && parentLevel !== null) ? parentLevel + 1 : 1;
            const levelVal = (this.activeLayout === 'hierarchical') ? calculatedLevel : null;

            nodes.add({
                id,
                label: finalLabel,
                name: '',
                x: parentPos.x + 50, y: parentPos.y + 50,
                image: this.getSvg(type, false, false, false, undefined, undefined, false),
                type, isRoot: false, isScreened: false,
                isFetched: false,
                isMainEntity: false,
                relationship: roleId,
                shareholderType: shType,
                shortId: shortId,
                level: levelVal
            });
            this.createEdge(parentId, id);
            this.hoveredNodeId = null;
            this.updateHierarchyTypes(id);
            this.autoLayout(this.activeLayout || 'hierarchical', true);

            // Open the property panel for the new node
            this.$nextTick(() => {
                const newNode = nodes.get(id);
                if (newNode) {
                    this.openNode(newNode);
                }
            });
        },
        deleteHoveredNode() {
            const node = this.nodes.get(this.hoveredNodeId);
            if (node && node.isScreened) {
                toastr.warning('Screened entities are locked and cannot be deleted.');
                return;
            }
            if (this.hoveredNodeId) {
                this.removeNodesAndEdges(this.hoveredNodeId);
                this.hoveredNodeId = null;
                this.updateBoundingBox();
                this.updateTree();
            }
        },
        deleteSelectedEdge() {
            if (this.selectedEdgeId) {
                const edge = this.edges.get(this.selectedEdgeId);
                if (edge) {
                    const targetNode = this.nodes.get(edge.to);
                    if (targetNode && targetNode.isScreened) {
                        toastr.warning('Relationships for screened entities are locked.');
                        return;
                    }
                    this.edges.remove(this.selectedEdgeId);
                    this.selectedEdgeId = null;
                    
                    const hasOtherParents = this.edges.get().some(e => e.to.toString() === edge.to.toString());
                    if (!hasOtherParents) {
                        this.nodes.update({
                            id: edge.to,
                            isRoot: true,
                            isMainEntity: true,
                            relationship: ''
                        });
                    }

                    this.updateNodeVisuals(edge.to);
                    this.updateHierarchyTypes(edge.to);
                }
            }
        },
        deleteSpecificEdge(edgeId) {
            const edge = this.edges.get(edgeId);
            if (edge) {
                const targetNode = this.nodes.get(edge.to);
                if (targetNode && targetNode.isScreened) {
                    toastr.warning('Relationships for screened entities are locked.');
                    return;
                }
                this.edges.remove(edgeId);

                const hasOtherParents = this.edges.get().some(e => e.to.toString() === edge.to.toString());
                if (!hasOtherParents) {
                    this.nodes.update({
                        id: edge.to,
                        isRoot: true,
                        isMainEntity: true,
                        relationship: ''
                    });
                }

                this.updateNodeVisuals(edge.to);
                this.updateHierarchyTypes(edge.to);
                this.saveDraft();
            }
        },
        updateEdgeData(edgeId, data) {
            const edges = Alpine.raw(this.edges);
            edges.update({
                id: edgeId,
                ...data
            });
            
            // Sync edge relationship to target node so it gets saved properly
            if (data.relationship) {
                const edge = edges.get(edgeId);
                if (edge && edge.to) {
                    const nodes = Alpine.raw(this.nodes);
                    const toNode = nodes.get(edge.to);
                    if (toNode) {
                        toNode.relationship = data.relationship;
                        const flagType = this.getRelationshipLabel(data.relationship);
                        nodes.update({ id: edge.to, relationship: data.relationship, flagType: flagType });
                        if (this.selectedNode && this.selectedNode.id === edge.to) {
                            this.formData.relationship = data.relationship;
                            this.formData.flagType = flagType;
                        }
                    }
                }
            }

            this.saveDraft();
        },
        updateEdgeRelationship(val) {
            if (!this.selectedEdgeId) return;
            const edges = Alpine.raw(this.edges);
            edges.update({
                id: this.selectedEdgeId,
                relationship: val,
                label: this.getRelationshipShortform(val)
            });

            const edge = this.edges.get(this.selectedEdgeId);
            if (edge) {
                const nodes = Alpine.raw(this.nodes);
                const targetNode = nodes.get(edge.to);
                if (targetNode) {
                    const role = this.RelatedPartyRoles.find(r => r.id === val);
                    if (role) {
                        const shType = role.type === 'C' ? 'Corporate' : 'Individual';
                        nodes.update({
                            id: edge.to,
                            type: role.type,
                            relationship: val,
                            shareholderType: shType
                        });
                        this.updateNodeVisuals(edge.to);
                        this.updateHierarchyTypes(edge.to);
                    }
                }
            }
            this.saveDraft();
        },
        reconnectEdge() {
            if (!this.selectedEdgeId) return;
            const edge = this.edges.get(this.selectedEdgeId);
            if (edge) {
                const targetNode = this.nodes.get(edge.to);
                if (targetNode && targetNode.isScreened) {
                    toastr.warning('Relationships for screened entities are locked.');
                    return;
                }
                const sourceId = edge.from;
                this.edges.remove(this.selectedEdgeId);
                this.selectedEdgeId = null;
                this.updateTree();
                this.startConnection(sourceId);
            }
        },
        async fetchExistingParty() {
            if (!this.searchId) return;
            Swal.fire({ title: 'Fetching...', didOpen: () => Swal.showLoading() });
            try {
                const r = await fetch(`/case/studio/fetch-party?id=${this.searchId}`);
                const result = await r.json();
                if (result && result.entity) {
                    const data = result.entity;
                    const attachments = result.attachments || [];
                    const nodes = Alpine.raw(this.nodes);
                    const nodeId = 'ext-' + this.searchId;
                    const caseId = data.customercode || data.customerCode || data.companyCode || data.CompanyCode || data.customerId || data.CustomerId || data.onb_cust_ref_id || data.onb_customerid || data.caseId || data.CaseId || data.id || data.Id || '';
                    const finalName = data.name || data.Name || [data.firstName, data.middleName, data.lastName].filter(Boolean).join(' ').trim() || 'Existing Entity';
                    const type = this.normalizeType(data.customerType || data.type || data.Type || 'I');
                    nodes.add({
                        ...this.formData,
                        ...data,
                        id: nodeId,
                        caseId: caseId,
                        metadata: data,
                        attachments: attachments,
                        level: (this.activeLayout === 'hierarchical') ? 0 : null,
                        label: finalName,
                        image: this.getSvg(type, false, true, false, caseId, undefined, true),
                        type: type,
                        isRoot: false,
                        isScreened: true,
                        isFetched: true,
                        isMainEntity: true
                    });
                    this.autoLayout();
                    Swal.close();
                }
            } catch (e) {
                console.error(e);
                Swal.fire('Fetch Error', 'Failed to retrieve entity data.', 'error');
            }
        },
        async clearCanvas(silent = false) {
            if (!silent) {
                const res = await Swal.fire({
                    title: 'Reset Canvas?',
                    text: 'This will remove all nodes and edges from the current workspace. This action cannot be undone.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#EF4444',
                    cancelButtonColor: '#64748B',
                    confirmButtonText: 'Yes, clear all',
                    cancelButtonText: 'Cancel'
                });
                if (!res.isConfirmed) return;
            }

            this.nodes.clear();
            this.edges.clear();
            this.groups = [];
            this.closeSheet();
            this.updateBoundingBox();
            if (!silent) toastr.success('Canvas has been reset.', 'Success');
        },
        closeSheet() {
            this.showSheet = false;
            this.selectedNode = null;
            this.formData = this.resetFormData();
        },
        removeNodesAndEdges(nodeIds) {
            if (!Array.isArray(nodeIds)) {
                nodeIds = [nodeIds];
            }
            const nodes = Alpine.raw(this.nodes);
            const edges = Alpine.raw(this.edges);

            const strNodeIds = nodeIds.map(id => id.toString());

            // 1. Find all connected edges
            const connectedEdges = edges.get().filter(e => 
                strNodeIds.includes(e.from.toString()) || 
                strNodeIds.includes(e.to.toString())
            );

            // 2. Identify orphaned children (children whose only parents are being deleted)
            const childrenLosingParents = connectedEdges
                .filter(e => strNodeIds.includes(e.from.toString()) && !strNodeIds.includes(e.to.toString()))
                .map(e => e.to.toString());

            const uniqueChildren = [...new Set(childrenLosingParents)];

            uniqueChildren.forEach(childId => {
                const remainingParents = edges.get().filter(e => 
                    e.to.toString() === childId && 
                    !strNodeIds.includes(e.from.toString())
                );
                if (remainingParents.length === 0) {
                    nodes.update({ 
                        id: childId, 
                        isRoot: true,
                        isMainEntity: true,
                        relationship: '' 
                    });
                    this.updateNodeVisuals(childId);
                    this.updateHierarchyTypes(childId);
                }
            });

            // 3. Remove edges
            if (connectedEdges.length > 0) {
                edges.remove(connectedEdges.map(e => e.id));
            }

            // 4. Remove nodes
            nodes.remove(nodeIds);

            // 5. autoLayout to adjust levels and recalculate
            this.autoLayout(this.activeLayout || 'hierarchical', true);
        },
        deleteNode() {
            if (this.selectedNode) {
                if (this.selectedNode.isScreened) {
                    toastr.warning('Screened entities are locked and cannot be deleted.');
                    return;
                }
                this.removeNodesAndEdges(this.selectedNode.id);
                this.closeSheet();
                this.updateTree();
            }
        },
        getSvg(type, isRoot, isScreened, isDuplicate, caseId, relationshipInput, isMainEntity = false) {
            const isCorporate = (type === 'C' || type === 'Corporate');
            
            // Node border/bg to look like the white cards in hovercard
            const nodeStroke = isRoot ? '#BFDBFE' : '#E2E8F0'; // Slightly blue border for roots
            const nodeBg = '#ffffff';

            const icons = {
                C_root: '<path d="M6 22V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v18"/><path d="M6 12H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h2"/><path d="M18 9h2a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-2"/><path d="M10 6h4"/><path d="M10 10h4"/><path d="M10 14h4"/><path d="M10 18h4"/>',
                I_root: '<path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>'
            };

            let actualRels = [];
            if (Array.isArray(relationshipInput)) {
                actualRels = relationshipInput;
            } else if (relationshipInput) {
                actualRels = [relationshipInput];
            }

            let relTexts = actualRels
                .filter(r => r && r !== 'root')
                .map(r => this.getRelationshipShortform(r))
                .filter(Boolean);
            
            // Get unique relationship shortforms
            relTexts = [...new Set(relTexts)];

            const getBadgeStyle = (relText) => {
                let badgeStyle = { text: '#2563EB', bg: '#EFF6FF', border: '#BFDBFE' }; // Default Blue (IRP/ISH)
                if (relText === 'CRP' || relText === 'CSH') {
                    badgeStyle = { text: '#16A34A', bg: '#F0FDF4', border: '#BBF7D0' }; // Green
                } else if (relText === 'AS') {
                    badgeStyle = { text: '#9333EA', bg: '#FAF5FF', border: '#E9D5FF' }; // Purple
                } else if (relText === 'SM') {
                    badgeStyle = { text: '#EA580C', bg: '#FFF7ED', border: '#FED7AA' }; // Orange
                }
                return badgeStyle;
            };

            // Main Entity styles
            const mainCorpStyle = { icon: '#2563EB', bg: '#EFF6FF' }; // Light Blue background, Blue icon
            const mainIndivStyle = { icon: '#16A34A', bg: '#F0FDF4' }; // Light Green background, Green icon

            const getCircleIcon = (cx, cy, r, style, text) => {
                return `<circle cx="${cx}" cy="${cy}" r="${r}" fill="${style.bg}" stroke="${style.border}" stroke-width="3"/><text x="${cx}" y="${cy + (r * 0.15)}" fill="${style.text}" font-family="Inter, sans-serif" font-weight="900" font-size="${r * 0.65}" text-anchor="middle">${text}</text>`;
            };

            const getMainIcon = (cx, cy, r, isC) => {
                const style = isC ? mainCorpStyle : mainIndivStyle;
                const pathStr = isC ? icons.C_root : icons.I_root;
                const scale = r * 0.055;
                const offset = r * 0.65;
                return `<circle cx="${cx}" cy="${cy}" r="${r}" fill="${style.bg}" stroke="none"/>
                        <g transform="translate(${cx - offset}, ${cy - offset}) scale(${scale})" stroke="${style.icon}" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round">${pathStr}</g>`;
            };

            // Dynamically layout visual items within the node
            let items = [];
            if (isRoot || isMainEntity || relTexts.length === 0) {
                items.push({ type: 'main' });
            }
            relTexts.forEach(rt => items.push({ type: 'rel', text: rt }));
            
            // Limit to 4 icons max for sane layout
            items = items.slice(0, 4);

            let iconPath = '';
            const renderItem = (item, cx, cy, r) => {
                if (item.type === 'main') {
                    return getMainIcon(cx, cy, r, isCorporate);
                } else {
                    return getCircleIcon(cx, cy, r, getBadgeStyle(item.text), item.text);
                }
            };

            if (items.length === 1) {
                iconPath = renderItem(items[0], 80, 80, 45);
            } else if (items.length === 2) {
                iconPath = renderItem(items[0], 50, 80, 32) + 
                           renderItem(items[1], 110, 80, 32);
            } else if (items.length === 3) {
                iconPath = renderItem(items[0], 80, 52, 28) +
                           renderItem(items[1], 50, 102, 28) +
                           renderItem(items[2], 110, 102, 28);
            } else if (items.length === 4) {
                iconPath = renderItem(items[0], 50, 50, 25) +
                           renderItem(items[1], 110, 50, 25) +
                           renderItem(items[2], 50, 110, 25) +
                           renderItem(items[3], 110, 110, 25);
            }

            const lockIcon = isScreened ? `<g transform="translate(120, 10)"><rect x="0" y="0" width="30" height="30" rx="8" fill="#10B981"/><path d="M10 14V10a5 5 0 0 1 10 0v4" stroke="white" stroke-width="2" fill="none"/><rect x="8" y="14" width="14" height="10" rx="2" fill="white"/></g>` : '';
            const dupIcon = isDuplicate ? `<g transform="translate(10, 10)"><rect x="0" y="0" width="30" height="30" rx="8" fill="#F59E0B"/><path d="M8 12H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v2" stroke="white" stroke-width="1.5" fill="none" transform="translate(6,6)"/><rect x="8" y="8" width="12" height="12" rx="2" stroke="white" stroke-width="1.5" fill="none" transform="translate(6,6)"/></g>` : '';

            const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="160" height="160" viewBox="0 0 160 160">
                <rect x="10" y="10" width="140" height="140" rx="32" fill="${nodeBg}" stroke="${nodeStroke}" stroke-width="4"/>
                ${iconPath}
                ${lockIcon}
                ${dupIcon}
            </svg>`;
            return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg.replace(/\s+/g, ' '));
        },
        updateNodeVisuals(nodeId) {
            const nodes = Alpine.raw(this.nodes);
            const edges = Alpine.raw(this.edges);
            const node = nodes.get(nodeId);
            if (!node) return;
            
            const incomingEdges = edges.get({ filter: e => e.to.toString() === nodeId.toString() });
            const relationships = incomingEdges.map(e => e.relationship).filter(Boolean);
            
            let relsToPass = [];
            if (relationships.length > 0) {
                relsToPass = relationships;
            } else if (!node.isRoot && node.relationship) {
                relsToPass = [node.relationship];
            }

            nodes.update({
                id: nodeId,
                image: this.getSvg(node.type || 'C', node.isRoot, node.isScreened, node.isDuplicate, node.caseId, relsToPass, !!(node.isMainEntity || node.isRoot))
            });
        },
        shouldShowMainForm(node) {
            if (!node) return false;
            return !!(node.isRoot || this.formData.isRoot || node.isMainEntity);
        },
        getTypeLabel(t) { return t === 'C' ? 'Corporate' : 'Individual'; },
        getIconForType(node) {
            if (!node) return 'user';
            return node.type === 'C' ? 'building-2' : 'user';
        },
        switchNodeType() {
            if (!this.selectedNode || this.selectedNode.isRoot) return;

            const nodes = Alpine.raw(this.nodes);
            const newType = this.formData.type === 'I' ? 'C' : 'I';

            // Map relationship to its counterpart
            const relMap = {
                'individual_shareholder': 'corporate_shareholder',
                'corporate_shareholder': 'individual_shareholder',
                'related_individual_shareholder': 'related_corporate_shareholder',
                'related_corporate_shareholder': 'related_individual_shareholder'
            };

            if (relMap[this.formData.relationship]) {
                this.formData.relationship = relMap[this.formData.relationship];
            }

            this.formData.type = newType;

            // Update node on canvas
            nodes.update({
                id: this.selectedNode.id,
                type: newType,
                image: this.getSvg(newType, false, this.formData.isScreened, this.selectedNode.isDuplicate, undefined, undefined, !!this.selectedNode.isFetched)
            });

            this.updateHierarchyTypes(this.selectedNode.id);

            // Update local selection reference
            this.selectedNode.type = newType;

            toastr.info(`Converted to ${this.getTypeLabel(newType)} Entity`);
            this.$nextTick(() => lucide.createIcons());
        },
        RelatedPartyRoles: [
            { id: 'related_individual_shareholder', label: 'Individual Related Parties', type: 'I' },
            { id: 'related_corporate_shareholder', label: 'Corporate Related Parties', type: 'C' },
            { id: 'individual_shareholder', label: 'Individual Shareholder', type: 'I' },
            { id: 'corporate_shareholder', label: 'Corporate Shareholder', type: 'C' },
            { id: 'authorized_signatory', label: 'Authorized Signatory', type: 'I' },
            { id: 'senior_management', label: 'Senior Management', type: 'I' }
        ],
        setRelatedPartyRole(roleId) {
            const role = this.RelatedPartyRoles.find(r => r.id === roleId);
            if (!role) return;
            this.formData.relationship = role.id;
            this.formData.type = role.type;
            this.formData.shareholderType = role.type === 'C' ? 'Corporate' : 'Individual';

            if (this.selectedNode) {
                this.nodes.update({
                    id: this.selectedNode.id,
                    type: role.type,
                    relationship: role.id,
                    shareholderType: this.formData.shareholderType
                });
                this.updateNodeVisuals(this.selectedNode.id);
                this.updateHierarchyTypes(this.selectedNode.id);
            }
            this.$nextTick(() => lucide.createIcons());
        },
        getRelationshipShortform(r) {
            if (!r) return 'ISH';
            const cleanR = r.toString().toLowerCase().trim();
            const map = {
                related_individual_shareholder: 'IRP',
                related_corporate_shareholder: 'CRP',
                individual_shareholder: 'ISH',
                corporate_shareholder: 'CSH',
                authorized_signatory: 'AS',
                senior_management: 'SM',
                irp: 'IRP',
                crp: 'CRP',
                ish: 'ISH',
                csh: 'CSH',
                as: 'AS',
                sm: 'SM',
                'individual related parties': 'IRP',
                'corporate related parties': 'CRP',
                'individual shareholder': 'ISH',
                'corporate shareholder': 'CSH',
                'authorized signatory': 'AS',
                'senior management': 'SM',
                'individual_ind': 'ISH',
                'corporate_ind': 'CSH',
                'individual_corp': 'ISH',
                'corporate_corp': 'CSH'
            };
            if (map[cleanR]) return map[cleanR];
            if (cleanR.includes('signatory') || cleanR.includes('as')) return 'AS';
            if (cleanR.includes('management') || cleanR.includes('sm')) return 'SM';
            if (cleanR.includes('related') && cleanR.includes('corp')) return 'CRP';
            if (cleanR.includes('related')) return 'IRP';
            if (cleanR.includes('corp')) return 'CSH';
            return 'ISH';
        },
        getSidebarBadgeClass(r) {
            const sf = this.getRelationshipShortform(r);
            if (sf === 'CRP' || sf === 'CSH') return 'bg-emerald-50 text-emerald-600 border-emerald-100';
            if (sf === 'AS') return 'bg-purple-50 text-purple-600 border-purple-100';
            if (sf === 'SM') return 'bg-amber-50 text-amber-600 border-amber-100';
            return 'bg-blue-50 text-blue-600 border-blue-100';
        },
        createEdge(fromId, toId) {
            const nodes = Alpine.raw(this.nodes);
            const edges = Alpine.raw(this.edges);
            const toNode = nodes.get(toId);
            if (!toNode) return;

            // Prevent duplicate edges
            const currentEdges = edges.get();
            if (currentEdges.some(e => (e.from.toString() === fromId.toString() && e.to.toString() === toId.toString()))) {
                toastr.warning('Relationship already exists.');
                return;
            }

            // Always derive the relationship from node type to prevent stale ISH on corporate nodes
            const derivedDefault = (toNode.type === 'C' || toNode.type === 'Corporate') ? 'corporate_shareholder' : 'individual_shareholder';
            const defaultSHType = (toNode.type === 'C' || toNode.type === 'Corporate') ? 'Corporate' : 'Individual';
            // Only override if it's missing or if the node is corporate but wrongly labeled as individual
            if (!toNode.relationship || toNode.relationship === 'root' || ((toNode.type === 'C' || toNode.type === 'Corporate') && toNode.relationship === 'individual_shareholder')) {
                nodes.update({
                    id: toId,
                    relationship: derivedDefault,
                    type: toNode.type || 'C',
                    shareholderType: defaultSHType
                });
                toNode.relationship = derivedDefault;
                toNode.shareholderType = defaultSHType;
            }

            let promoted = false;
            if (toNode && toNode.isRoot) {
                // Promote the new parent to root if it doesn't already have its own parent
                const hasOtherParent = edges.get().some(e => e.to.toString() === fromId.toString());
                if (!hasOtherParent) {
                    nodes.update({ 
                        id: fromId, 
                        isRoot: true,
                        isMainEntity: true
                    });
                    if (this.selectedNode && this.selectedNode.id === fromId) {
                        this.formData.isRoot = true;
                        this.formData.isMainEntity = true;
                    }
                    promoted = true;
                }
                // Demote the target node since it now has a parent and is no longer a root
                nodes.update({ 
                    id: toId, 
                    isRoot: false
                });
                toNode.isRoot = false;
                if (this.selectedNode && this.selectedNode.id === toId) this.formData.isRoot = false;
            }

            edges.add({
                from: fromId,
                to: toId,
                relationship: toNode.relationship,
                share: toNode.share || null,
                designation: toNode.designation || '',
                remarks: toNode.remarks || '',
                label: this.getRelationshipShortform(toNode.relationship),
                font: { align: 'middle', size: 11, face: 'Inter', strokeWidth: 4, strokeColor: '#ffffff', color: '#64748B', weight: 'bold' },
                arrows: { from: { enabled: true, scaleFactor: 0.5 } }
            });

            if (promoted) {
                this.updateHierarchyTypes(fromId);
            } else {
                this.updateHierarchyTypes(toId);
            }

            // Group Sync: If source is in a group and target is not, or vice versa, offer to sync or auto-sync
            // For now, if source is in any group, and we connect target, target remains independent unless manually added.
            // But we must ensure if we move a node, group badges follow (already handled by updateGroupOverlays)

            this.updateTree();
        },
        getRelationshipLabel(r) {
            const map = {
                related_individual_shareholder: 'Individual Related Parties',
                related_corporate_shareholder: 'Corporate Related Parties',
                individual_shareholder: 'Individual Shareholder',
                corporate_shareholder: 'Corporate Shareholder',
                authorized_signatory: 'Authorized Signatory',
                senior_management: 'Senior Management'
            };
            if (!r || r === 'root') return 'Main Entity';
            return map[r] || r.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
        },
        getRelationshipEnum(label) {
            if (!label) return '';
            const lowerLabel = label.toString().toLowerCase().trim();
            if (lowerLabel.includes('individual') && lowerLabel.includes('related')) return 'related_individual_shareholder';
            if (lowerLabel.includes('corporate') && lowerLabel.includes('related')) return 'related_corporate_shareholder';
            if (lowerLabel.includes('individual')) return 'individual_shareholder';
            if (lowerLabel.includes('corporate')) return 'corporate_shareholder';
            if (lowerLabel.includes('signatory')) return 'authorized_signatory';
            if (lowerLabel.includes('senior') || lowerLabel === 'sm') return 'senior_management';
            return '';
        },
        async saveCase() {
            if (this.isSubmitting) return;
            if (!this.validateAllNodes()) return;

            const res = await Swal.fire({
                title: 'Finalize Studio?',
                text: 'Submit all entities for screening and case creation?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, Create Case',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#2563eb',
                cancelButtonColor: '#f1f5f9',
                customClass: {
                    confirmButton: 'rounded-xl px-8 py-3 font-bold',
                    cancelButton: 'rounded-xl px-8 py-3 font-bold text-slate-600'
                }
            });

            if (res.isConfirmed) {
                this.isSubmitting = true;
                // Show Global Loader
                if (window.$) $('#loadingeffect').fadeIn(200);
                this.isLoading = true;

                const rawNodes = Alpine.raw(this.nodes).get();
                const rawEdges = Alpine.raw(this.edges).get();

                const finalNodes = rawNodes.map(node => {
                    const dto = this.mapNodeToDto(node);
                    dto.StudioId = node.id.toString();
                    
                    const incomingEdges = rawEdges.filter(e => e.to.toString() === node.id.toString());
                    if (incomingEdges.length > 0 && !node.isMainEntity) {
                        const firstEdge = incomingEdges[0];
                        const parentNode = rawNodes.find(n => n.id.toString() === firstEdge.from.toString());
                        const pType = parentNode ? (parentNode.type === 'C' ? 'Corporate' : 'Individual') : 'Corporate';
                        const base = node.type === 'C' ? 'Corp' : 'Ind';
                        const newType = `${pType}_${base}`;
                        dto.ShareholderType = newType.split('_').length > 8 ? newType.split('_').slice(0, 8).join('_') : newType;
                        dto.Type = dto.ShareholderType;
                    } else {
                        dto.ShareholderType = node.type === 'C' ? 'Corporate' : 'Individual';
                        dto.Type = dto.ShareholderType;
                    }
                    return dto;
                });

                const finalEdges = rawEdges.map(edge => ({
                    From: edge.from.toString(),
                    To: edge.to.toString(),
                    Relationship: edge.relationship || '',
                    Share: edge.share || null,
                    Designation: edge.designation || '',
                    Remarks: edge.remarks || '',
                    Label: edge.label || ''
                }));


                try {
                    const response = await fetch('/case/studio/batch-create', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            DraftId: this.activeDraftId,
                            Nodes: finalNodes,
                            Edges: finalEdges
                        })
                    });

                    if (!response.ok) {
                        const errorMsg = await response.text();
                        toastr.error(errorMsg || `Server error (${response.status})`);
                        this.isSubmitting = false;
                        if (window.$) $('#loadingeffect').fadeOut(200);
                        this.isLoading = false;
                        return;
                    }

                    const data = await response.json();

                    if (data.success) {
                        toastr.success(data.message || 'Case(s) created successfully');

                        // Set success modal state
                        this.isSuccessModalOpen = true;
                        this.caseRefId = data.message.match(/ID:\s*(\w+)/)?.[1] || data.message || 'Success';

                        // Auto-hide loader
                        if (window.$) $('#loadingeffect').fadeOut(200);
                        this.isLoading = false;

                        // Note: Redirection is now handled by the modal buttons
                    } else {
                        this.isSubmitting = false;
                        if (window.$) $('#loadingeffect').fadeOut(200);
                        this.isLoading = false;
                        toastr.error(data.message || 'Submission failed. Please check mandatory fields or connection.');

                        // If there are specific validation errors returned from backend
                        if (data.errors && data.errors.length > 0) {
                            console.error('Backend Validation Errors:', data.errors);
                        }
                    }
                } catch (err) {
                    this.isSubmitting = false;
                    if (window.$) $('#loadingeffect').fadeOut(200);
                    this.isLoading = false;
                    toastr.error('An unexpected error occurred during submission.');
                    console.error('Submission Error:', err);
                }
            }
        },
        getStatusClass(text) {
            if (!text) return 'bg-slate-100 text-slate-800';
            const t = text.toLowerCase().trim();
            if (t.includes('pending')) return 'bg-amber-100 text-amber-800';
            if (t.includes('approved')) return 'bg-emerald-600 text-white shadow-sm shadow-emerald-100';
            if (t.includes('auto')) return 'bg-emerald-100 text-emerald-800';
            if (t.includes('transfer')) return 'bg-emerald-50 text-emerald-700';
            if (t.includes('rejected')) return 'bg-rose-100 text-rose-800 border border-rose-200';
            return 'bg-slate-100 text-slate-800';
        },
        closeSuccess() {
            this.isSuccessModalOpen = false;
            window.location.reload();
        },
        getProcessUrl(item) {
            const isShareholder = !(item.type === 'Individual' || item.type === 'Corporate');
            return isShareholder
                ? `/report/ViewShareholderCaseDetails?id=${item.customerId}&type=1`
                : `/report/ViewIndividualCaseDetail?id=${item.customerId}&type=${item.customerType}`;
        }
    };
}
