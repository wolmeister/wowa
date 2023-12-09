
module main


import os
import net.http
import json
import io.util
import szip

import kv
import command

struct CursePagination {
	index        int
	page_size    int [json: 'pageSize']
	result_count int [json: 'resultCount']
	total_count  int [json: 'totalCount']
}

struct CurseFileModule {
	name        string
	fingerprint int
}

struct CurseFile {
	id      int
	game_id int [json: 'gameId']
	mod_id  int [json: 'modId']
	// is_Available: boolean;
	display_name string [json: 'displayName']
	// file_Name: string;
	//   releaseType: CF2FileReleaseType;
	//   fileStatus: CF2FileStatus;
	//   hashes: CF2FileHash[];
	// file_Date: string;
	// file_Length: number;
	// download_Count: number;
	download_url string [json: 'downloadUrl']
	// game_Versions: string[];
	//   sortableGameVersions: CF2SortableGameVersion[];
	//   dependencies: CF2FileDependency[];
	// exposeAsAlternative?: boolean;
	// parentProjectFileId?: number;
	// alternateFileId?: number;
	// isServerPack?: number;
	// serverPackFileId?: number;
	// fileFingerprint: number;
	modules []CurseFileModule
}

struct CurseFileIndex {
	game_version         string [json: 'gameVersion']
	file_id              int    [json: 'fileId']
	filename             string
	release_type         int    [json: 'releaseType']
	game_version_type_id int    [json: 'gameVersionTypeId']
	// modLoader?: CF2ModLoaderType;
}

struct CurseAddon {
	id      int
	game_id int    [json: 'gameId']
	name    string
	slug    string
	//   links CF2AddonLinks
	summary string
	//   status CF2ModStatus
	download_count      int  [json: 'downloadCount']
	is_featured         bool [json: 'isFeatured']
	primary_category_id int  [json: 'primaryCategoryId']
	//   categories CF2Category[]
	//   authors CF2Author[]
	//   logo CF2Asset
	//   screenshots CF2Asset[]
	main_file_id           int              [json: 'mainFileId']
	latest_files           []CurseFile      [json: 'latestFiles']
	latest_files_indexes   []CurseFileIndex [json: 'latestFilesIndexes']
	date_created           string           [json: 'dateCreated']
	date_modified          string           [json: 'dateModified']
	date_released          string           [json: 'dateReleased']
	allow_mod_distribution bool             [json: 'allowModDistribution']
	game_popularity_rank   int              [json: 'gamePopularityRank']
	is_available           bool             [json: 'isAvailable']
	thumbs_up_count        int              [json: 'thumbsUpCount']
}

struct CurseSearchModsResponse {
	data       []CurseAddon
	pagination CursePagination
}

struct CurseAddonFileResponse {
	data CurseFile
}

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



fn get_db_path() string {
	config_dir := os.config_dir() or { panic(err) }
	return os.join_path(config_dir, 'wowa', 'wowa.json')
}

fn main() {
	// println('Hello World!')
	// println(os.args)

	if os.args.len == 1 {
		println('No command provided')
		return
	}

	db_path := get_db_path()
	mut store := kv.init_store(db_path) or { panic(err) }

	println("Using db PATH ${db_path}")
	println("Args: ${os.args}")

	// println(get_db_path())

	command_arg := os.args[1]

	if command_arg == 'config' {
		if os.args.len == 2 {
			println("missing name")
			return
		}

		if os.args.len == 3 {
			value := store.get(['config',os.args[2]]) or { 'null' }
			println(value)
			return
		}

		store.set(['config', os.args[2]], os.args[3])!
		return
	}

	if command_arg == 'ls' {
		command.execute_ls_command(store, os.args) or { panic(err) }
		return
	}

	if command_arg != 'add' {
		println('invalid command')
		return
	}

	slug := os.args[2]
	classic := os.args.len == 4 && os.args[3] == '--classic'
	game_version_type_id :=  if classic { 67408 } else { 517 };

	println('Is classic: ${classic}')

	game_dir := store.get(['config', 'game.dir']) or { panic('Missing game.dir prop') }
	curse_token := store.get(['config', 'curse.token']) or { panic('Missing curse.token prop') }

	println('Searching for addon ${slug}')
	req := http.FetchConfig{
		method: .get
		url: 'https://api.curseforge.com/v1/mods/search?gameId=1&slug=${slug}&sortField=2&sortOrder=desc&index=0&gameVersionTypeId=${game_version_type_id}'
		header: http.new_custom_header_from_map({
			'x-api-key': curse_token
		})!
	}
	res := http.fetch(req) or { panic(err) }
	println("Request status ${res.status_code}")
	body := json.decode(CurseSearchModsResponse, res.body) or { panic(err) }

	println("Searching for file index")
	addon := body.data.filter(it.slug == slug).first()
	file_index := addon.latest_files_indexes.filter(it.game_version_type_id == game_version_type_id
		&& it.release_type == 1).first()

	println("Fetching file")
	req2 := http.FetchConfig{
		method: .get
		url: 'https://api.curseforge.com/v1/mods/${addon.id}/files/${file_index.file_id}'
		header: http.new_custom_header_from_map({
			'x-api-key': curse_token
		})!
	}

	res2 := http.fetch(req2) or { panic(err) }
	body2 := json.decode(CurseAddonFileResponse, res2.body) or { panic(err) }

	mut zip_file, zip_file_path := util.temp_file() or { panic(err) }
	zip_file.close()

	println("Downloading file")
	http.download_file(body2.data.download_url, zip_file_path) or { panic(err) }

	version_folder := if classic { '_classic_era_' } else { '_retail_' }
	addons_folder := os.join_path(game_dir, '/${version_folder}/Interface/Addons')
	if os.exists(addons_folder) == false {
		println('Creating addons folder')
		os.mkdir_all(addons_folder)!
	}

	println("Extracting file")
	szip.extract_zip_to_dir(zip_file_path, addons_folder)!

	os.rm(zip_file_path)!

	store.set(['addons', if classic { 'classic' } else { 'retail' }, slug], json.encode(Addon{
		id: slug
		name: addon.name
		author: 'author'
		version: body2.data.display_name
		game_version: if classic { 'classic' } else { 'retail' }
		directories: body2.data.modules.map(it.name)
		source: AddonSource{
			provider: 'curse'
			id: addon.id.str()
			url: 'https://www.curseforge.com/wow/addons/${slug}'
		}
	})) or { panic(err) }
}
