// Copyright 2022 Carl Reinke
//
// This file is part of a library that is licensed under the terms of the GNU
// Lesser General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor.CodeCleanup;
using Microsoft.VisualStudio.Language.CodeCleanUp;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FormatDocComments
{
    [Export]
    internal sealed class CodeCleanUpFixer : ICodeCleanUpFixer
    {
        internal const string FormatDocCommentsFixId = nameof(FormatDocCommentsFixId);

        private readonly FormatDocCommentsService _formatDocCommentsService;
        private readonly JoinableTaskContext _joinableTaskContext;
        private readonly IVsHierarchyItemManager _vsHierarchyItemManager;
        private readonly VisualStudioWorkspace _workspace;

        [ImportingConstructor]
        public CodeCleanUpFixer(FormatDocCommentsService formatDocCommentsService, JoinableTaskContext joinableTaskContext, IVsHierarchyItemManager vsHierarchyItemManager, VisualStudioWorkspace workspace)
        {
            _formatDocCommentsService = formatDocCommentsService;
            _joinableTaskContext = joinableTaskContext;
            _vsHierarchyItemManager = vsHierarchyItemManager;
            _workspace = workspace;
        }

        public Task<bool> FixAsync(ICodeCleanUpScope scope, ICodeCleanUpExecutionContext context)
        {
            switch (scope)
            {
                case TextBufferCodeCleanUpScope textBufferScope:
                    return FixAsync(textBufferScope, context);
                case IVsHierarchyCodeCleanupScope hierarchyScope:
                    return FixAsync(hierarchyScope, context);
                default:
                    return Task.FromResult(false);
            }
        }

        private async Task<bool> FixAsync(TextBufferCodeCleanUpScope textBufferScope, ICodeCleanUpExecutionContext context)
        {
            var textBuffer = textBufferScope.SubjectBuffer;
            if (textBuffer.EditInProgress)
                return false;

            var document = textBuffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return false;

            document = await _formatDocCommentsService.FormatDocCommentsInDocumentAsync(document, context.OperationContext.UserCancellationToken);

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(context.OperationContext.UserCancellationToken);

            return _workspace.TryApplyChanges(document.Project.Solution);
        }

        private async Task<bool> FixAsync(IVsHierarchyCodeCleanupScope hierarchyScope, ICodeCleanUpExecutionContext context)
        {
            var hierarchy = hierarchyScope.Hierarchy;
            if (hierarchy == null)
            {
                return await FixAsync(_workspace.CurrentSolution, context).ConfigureAwait(false);
            }

            uint itemId = hierarchyScope.ItemId;

            if (itemId == (uint)VSConstants.VSITEMID.Root)
            {
                var project = GetProjectByHierarchy(hierarchy);
                if (project == null)
                    return false;

                return await FixAsync(project, context).ConfigureAwait(false);
            }

            var item = _vsHierarchyItemManager.GetHierarchyItem(hierarchy, itemId);

            var document = GetDocumentByHierarchyAndPath(hierarchy, item.CanonicalName);
            if (document == null)
                return false;

            return await FixAsync(document, context).ConfigureAwait(false);
        }

        private Project GetProjectByHierarchy(IVsHierarchy hierarchy)
        {
            foreach (var project in _workspace.CurrentSolution.Projects)
                if (_workspace.GetHierarchy(project.Id) == hierarchy)
                    return project;

            return null;
        }

        private Document GetDocumentByHierarchyAndPath(IVsHierarchy hierarchy, string filePath)
        {
            var documentIds = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath);

            foreach (var documentId in documentIds)
                if (_workspace.GetHierarchy(documentId.ProjectId) == hierarchy)
                    return _workspace.CurrentSolution.GetDocument(documentId);

            return null;
        }

        private async Task<bool> FixAsync(Solution solution, ICodeCleanUpExecutionContext context)
        {
            solution = await FixAsync(ApplyFixAsync, solution, context).ConfigureAwait(false);

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(context.OperationContext.UserCancellationToken);

            return solution.Workspace.TryApplyChanges(solution);
        }

        private async Task<Solution> ApplyFixAsync(Solution solution, ReportProgress reportProgress, ICodeCleanUpExecutionContext context, CancellationToken cancellationToken)
        {
            var projectIds = solution.ProjectIds;

            for (int i = 0; i < projectIds.Count; ++i)
            {
                var project = solution.GetProject(projectIds[i]);

                reportProgress(project.Name, i, projectIds.Count);

                cancellationToken.ThrowIfCancellationRequested();

                project = await FixAsync(ApplyFixAsync, project, context).ConfigureAwait(false);

                solution = project.Solution;
            }

            reportProgress(null, projectIds.Count, projectIds.Count);

            return solution;
        }

        private async Task<bool> FixAsync(Project project, ICodeCleanUpExecutionContext context)
        {
            project = await FixAsync(ApplyFixAsync, project, context).ConfigureAwait(false);

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(context.OperationContext.UserCancellationToken);

            var solution = project.Solution;
            return solution.Workspace.TryApplyChanges(solution);
        }

        private async Task<Project> ApplyFixAsync(Project project, ReportProgress reportProgress, ICodeCleanUpExecutionContext context, CancellationToken cancellationToken)
        {
            var documentIds = project.DocumentIds;

            for (int i = 0; i < documentIds.Count; ++i)
            {
                var document = project.GetDocument(documentIds[i]);

                reportProgress(document.Name, i, documentIds.Count);

                document = await FixAsync(document, context.EnabledFixIds, cancellationToken).ConfigureAwait(false);

                project = document.Project;
            }

            reportProgress(null, documentIds.Count, documentIds.Count);

            return project;
        }

        private async Task<bool> FixAsync(Document document, ICodeCleanUpExecutionContext context)
        {
            document = await FixAsync(ApplyFixAsync, document, context).ConfigureAwait(false);

            await _joinableTaskContext.Factory.SwitchToMainThreadAsync(context.OperationContext.UserCancellationToken);

            var solution = document.Project.Solution;
            return solution.Workspace.TryApplyChanges(solution);
        }

        private async Task<Document> ApplyFixAsync(Document document, ReportProgress reportProgress, ICodeCleanUpExecutionContext context, CancellationToken cancellationToken)
        {
            reportProgress(document.Name, 0, 1);

            document = await FixAsync(document, context.EnabledFixIds, cancellationToken).ConfigureAwait(false);

            reportProgress(null, 1, 1);

            return document;
        }

        private async Task<T> FixAsync<T>(Func<T, ReportProgress, ICodeCleanUpExecutionContext, CancellationToken, Task<T>> applyFixAsync, T value, ICodeCleanUpExecutionContext context)
        {
            using (var scope = context.OperationContext.AddScope(allowCancellation: true, "Applying changes"))
            {
                return await applyFixAsync(value, ReportProgress, context, scope.Context.UserCancellationToken).ConfigureAwait(false);

                void ReportProgress(string description, int completedItems, int totalItems)
                {
                    scope.Description = description;
                    scope.Progress.Report(new Microsoft.VisualStudio.Utilities.ProgressInfo(completedItems, totalItems));
                }
            }
        }

        private async Task<Document> FixAsync(Document document, FixIdContainer enabledFixIds, CancellationToken cancellationToken)
        {
            if (enabledFixIds.IsFixIdEnabled(FormatDocCommentsFixId))
                document = await _formatDocCommentsService.FormatDocCommentsInDocumentAsync(document, cancellationToken).ConfigureAwait(false);

            return document;
        }

        private delegate void ReportProgress(string description, int completedItems, int totalItems);
    }
}
