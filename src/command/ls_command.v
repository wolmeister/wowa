module command

import kv
import json
import math

struct AddonSource {
	provider string
	id       string
	url      string
}

struct Addon {
	id           string
	name         string
	author       string
	directories  []string
	version      string
	game_version string      [json: 'gameVersion']
	source       AddonSource
}

pub fn execute_ls_command(store kv.KeyValueStore, args []string) ! {
	addons_string := store.get_by_prefix(['addons'])
	addons := addons_string.map(json.decode(Addon, it) or { return err })

	mut table := [['ID', 'Name', 'Version', 'Game Version']]
	mut largest_columns := table[0].map(it.len)
	for addon in addons {
		table.insert(table.len, [addon.id, addon.name, addon.version, addon.game_version])
		largest_columns[0] = math.max(addon.id.len, largest_columns[0])
		largest_columns[1] = math.max(addon.name.len, largest_columns[1])
		largest_columns[2] = math.max(addon.version.len, largest_columns[2])
		largest_columns[3] = math.max(addon.game_version.len, largest_columns[3])
	}

	for column_size in largest_columns {
		for _ in 0..(column_size + 3) {
			print('-')
		}
	}
	print('-\n')


	for row in table {
		for column_index, column in row {
			print('| ')
			print(column)

			for _ in column.len..(largest_columns[column_index] + 1) {
				print(' ')
			}

			if column_index == row.len - 1 {
				print('|')
			}

		}
		print('\n')
	}

	for column_size in largest_columns {
		for _ in 0..(column_size + 3) {
			print('-')
		}
	}
	print('-\n')
}
