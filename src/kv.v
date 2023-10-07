module kv

import os
import json

struct KeyValueEntry {
	key   []string
	value string
}

pub struct KeyValueStore {
	path string [required]
mut:
	entries []KeyValueEntry [required]
}

pub fn init_store(path string) !KeyValueStore {
	mut entries  := []KeyValueEntry{}

	// Init the store if the file does not exists yet
	if os.exists(path) == false {
		if os.exists(os.dir(path)) == false {
			os.mkdir_all(os.dir(path)) or { return err }
		}
		mut file := os.create(path) or { return err }
		file.write_string('[]') or { return err }
		file.close()
	} else {
		// Initialize the in-memory entries
		json_string := os.read_file(path) or { return err }
		entries = json.decode([]KeyValueEntry, json_string) or { return err }
	}

	return KeyValueStore{
		path: path
		entries: entries
	}
}

pub fn (store KeyValueStore) get(key []string) ?string {
	entry := store.entries.filter(it.key == key)
	if entry.len == 1 {
		return entry.first().value
	}
	return none
}

pub fn (store KeyValueStore) get_by_prefix(prefix []string) []string {
	entries := store.entries.filter(it.key[0..prefix.len] == prefix)
	return entries.map(it.value)
}

pub fn (mut store KeyValueStore) set(key []string, value string) ! {
	// Update the in-memory entries
	store.entries = store.entries.filter(it.key != key)
	store.entries.insert(store.entries.len,KeyValueEntry{
		key: key
		value: value
	})

	// Save to disk
	result := json.encode(store.entries)

	mut file := os.open_file(store.path, 'w') or { return err }
	file.write_string(result) or { return err }
	file.close()
}
