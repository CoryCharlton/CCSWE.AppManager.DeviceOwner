using System.Collections.ObjectModel;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// In-place reconcile of an <see cref="ObservableCollection{T}"/> of row view models against a fresh snapshot,
/// keyed by a stable identity, so selection and transient row state survive a refresh.
/// </summary>
public static class ObservableCollectionMergeExtensions
{
    /// <summary>
    /// Adds rows for new items, updates existing rows in place (matched by key), removes rows whose item is gone,
    /// then reorders the collection to match <paramref name="sortKey"/> (case-insensitive) using minimal moves.
    /// </summary>
    public static void MergeBy<TItem, TRow>(
        this ObservableCollection<TRow> rows,
        IReadOnlyList<TItem> items,
        Func<TItem, string> itemKey,
        Func<TRow, string> rowKey,
        Func<TItem, TRow> createRow,
        Action<TRow, TItem> updateRow,
        Func<TRow, string> sortKey)
    {
        foreach (var item in items)
        {
            var key = itemKey(item);
            var existing = rows.FirstOrDefault(row => rowKey(row) == key);

            if (existing is null)
            {
                rows.Add(createRow(item));
            }
            else
            {
                updateRow(existing, item);
            }
        }

        for (var i = rows.Count - 1; i >= 0; i--)
        {
            var key = rowKey(rows[i]);
            if (items.All(item => itemKey(item) != key))
            {
                rows.RemoveAt(i);
            }
        }

        var ordered = rows.OrderBy(sortKey, StringComparer.OrdinalIgnoreCase).ToList();
        for (var target = 0; target < ordered.Count; target++)
        {
            var current = rows.IndexOf(ordered[target]);
            if (current != target)
            {
                rows.Move(current, target);
            }
        }
    }
}
