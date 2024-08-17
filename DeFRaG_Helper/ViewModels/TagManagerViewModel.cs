using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DeFRaG_Helper.ViewModels
{
    public class TagManagerViewModel : BaseViewModel
    {
        public ObservableCollection<TagTextItem> Tags { get; set; } = new ObservableCollection<TagTextItem>();
        public string NewTagName { get; set; } = string.Empty;
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand TagCheckedCommand { get; }
        public ICommand TagUncheckedCommand { get; }

        private readonly Map _map;
        public event EventHandler? RequestClose;

        public TagManagerViewModel(Map map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            AddTagCommand = new RelayCommand(AddTag);
            RemoveTagCommand = new RelayCommand(RemoveTag);
            CancelCommand = new RelayCommand(Cancel);
            TagCheckedCommand = new RelayCommand(OnTagChecked);
            TagUncheckedCommand = new RelayCommand(OnTagUnchecked);

            LoadTags();
        }

        private async void LoadTags()
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand("SELECT Tag FROM Tag", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string tag = reader.GetString(0);
                            bool isChecked = _map.Tags.Contains(tag);
                            App.Current.Dispatcher.Invoke(() => Tags.Add(new TagTextItem { Name = tag, IsChecked = isChecked }));
                        }
                    }
                }
            });
            await DbQueue.Instance.WhenAllCompleted();
        }

        private void AddTag(object parameter)
        {
            if (!string.IsNullOrEmpty(NewTagName) && !Tags.Any(t => t.Name == NewTagName))
            {
                var newTagItem = new TagTextItem { Name = NewTagName, IsChecked = true };
                Tags.Add(newTagItem);
                _map.Tags.Add(NewTagName);
                AddTagToDatabase(NewTagName);

                // Update TagBarViewModel
                TagBarViewModel.Instance.AddTag(new TagItem { Name = NewTagName });
            }
        }

        private void RemoveTag(object parameter)
        {
            if (parameter is TagTextItem tagItem && Tags.Contains(tagItem))
            {
                Tags.Remove(tagItem);
                _map.Tags.Remove(tagItem.Name);
                DeleteTagFromDatabase(tagItem.Name);

                // Update TagBarViewModel
                TagBarViewModel.Instance.RemoveTag(tagItem.Name);
            }
        }

        private void DeleteTagFromDatabase(string tagName)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                // Delete the tag from the MapTag table
                using (var deleteMapTagCommand = new SqliteCommand("DELETE FROM MapTag WHERE TagID = (SELECT TagID FROM Tag WHERE Tag = @tag)", connection))
                {
                    deleteMapTagCommand.Parameters.AddWithValue("@tag", tagName);
                    await deleteMapTagCommand.ExecuteNonQueryAsync();
                }

                // Delete the tag from the Tag table
                using (var deleteTagCommand = new SqliteCommand("DELETE FROM Tag WHERE Tag = @tag", connection))
                {
                    deleteTagCommand.Parameters.AddWithValue("@tag", tagName);
                    await deleteTagCommand.ExecuteNonQueryAsync();
                }
            });
        }



        private void Cancel(object parameter)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void AddTagToDatabase(string tagName)
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                // Check if the tag already exists in the Tag table
                using (var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Tag WHERE Tag = @tag", connection))
                {
                    checkCommand.Parameters.AddWithValue("@tag", tagName);
                    var count = (long)await checkCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        // Insert the new tag into the Tag table
                        using (var insertCommand = new SqliteCommand("INSERT INTO Tag (Tag) VALUES (@tag)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@tag", tagName);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Insert the tag into the MapTag table
                using (var command = new SqliteCommand("INSERT INTO MapTag (MapID, TagID) VALUES (@mapId, (SELECT TagID FROM Tag WHERE Tag = @tag))", connection))
                {
                    command.Parameters.AddWithValue("@mapId", _map.Id);
                    command.Parameters.AddWithValue("@tag", tagName);
                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        private void UpdateTagsInDatabase()
        {
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand("DELETE FROM MapTag WHERE MapID = @mapId", connection))
                {
                    command.Parameters.AddWithValue("@mapId", _map.Id);
                    await command.ExecuteNonQueryAsync();
                }

                foreach (var tag in _map.Tags)
                {
                    using (var command = new SqliteCommand("INSERT INTO MapTag (MapID, TagID) VALUES (@mapId, (SELECT TagID FROM Tag WHERE Tag = @tag))", connection))
                    {
                        command.Parameters.AddWithValue("@mapId", _map.Id);
                        command.Parameters.AddWithValue("@tag", tag);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            });
        }

        private void OnTagChecked(object parameter)
        {
            if (parameter is TagTextItem tag && !_map.Tags.Contains(tag.Name))
            {
                _map.Tags.Add(tag.Name);
                UpdateTagsInDatabase();
            }
        }

        private void OnTagUnchecked(object parameter)
        {
            if (parameter is TagTextItem tag && _map.Tags.Contains(tag.Name))
            {
                _map.Tags.Remove(tag.Name);
                UpdateTagsInDatabase();
            }
        }
    }

    public class TagTextItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}
