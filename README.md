_______
English
=======
English is not my native language, so the text may not have machine translation accuracy, I'm sorry.
_______
This Jellyfin plugin mirror is intended for those who, for some reason, have the provider block the sites of the original repositories.


There are 2 types of mirrors in total.:

	1. Repository catalog only

	2. The repository catalog and all its plugins


Also, each type is divided into 3 types according to the plugins it contains.

	1. Plugins from the official repository

	2. Plugins from unofficial repositories

	3. Plugins from all repositories


Mirror of the official repository catalog:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-official-plugins.json


Mirror of the unofficial repository catalog:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-3rd-party-plugin.json


Mirror of official and unofficial repository catalogs:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-in-one-plugins.json


Mirror of the official repository catalog with all the plugins included in it:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-official-plugins.json


Mirror of a catalog of unofficial repositories with all the plugins included in them:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-3rd-party-plugin.json


Mirror of official and unofficial repository catalogs with all their plugins:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-in-one-plugins.json


A list of all repositories included in the mirror:

[repo-list.txt](http://hiranokohta.github.io/mirror-jellyfin-plugins-repo/repo-list.txt)


Log of the latest update of the repository and plugin mirrors:

[mirror-log.html](http://hiranokohta.github.io/mirror-jellyfin-plugins-repo/mirror-log.html)
_______
If the plug-in section does not open in your Jellyfin server settings (this may be due to blocking the resource where the repository is located), then you can manually edit the file. Jellyfin/config/system.xml

To do this, do the following:

1. Stop the Jellyfin server.

2. Open the settings file specified above.

3. Find the section responsible for the repository.

4. Enter a link to an alternative repository there.

5. Close the file by saving the changes.

6. Launch the Jellyfin server.
```
<PluginRepositories>
	<RepositoryInfo>
		<Name>Mirror All In One Plugins</Name>
		<Url>https://repo.jellyfin.org/files/plugin/manifest.json</Url>
		<Enabled>true</Enabled>
	</RepositoryInfo>
</PluginRepositories>
```
_______
_______
Russian
=======

Это зеркало плагинов Jellyfin предназначено для тех, у кого по каким либо причинам провайдер блокирует сайты оригинальных репозиториев.


Всего здесь находится 2 типа зеркал:

	1. Только каталог репозитория

	2. Каталог репозитория и все входящие в него плагины


Так же каждый тип делится на 3 вида по содержащимся в нем плагинам

	1. Плагины из официального репозитория

	2. Плагины из неофициальных репозиториев

	3. Плагины из всех репозиториев


Зеркало каталога официального репозитория:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-official-plugins.json


Зеркало каталога неофициальных репозиториев:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-3rd-party-plugin.json


Зеркало каталогов официального и неофициальных репозиториев:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/all-in-one-plugins.json


Зеркало каталога официального репозитория со всеми входящими в него плагинами:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-official-plugins.json


Зеркало каталога неофициальных репозиториев со всеми входящими в них плагинами:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-3rd-party-plugin.json


Зеркало каталогов официального и неофициальных репозиториев со всеми входящими в них плагинами:

	https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main/mirror-all-in-one-plugins.json


Список всех репозиториев входящих в зеркало:

[repo-list.txt](http://hiranokohta.github.io/mirror-jellyfin-plugins-repo/repo-list.txt)


Журнал последнего обновления зеркала репозиториев и плагинов:

[mirror-log.html](http://hiranokohta.github.io/mirror-jellyfin-plugins-repo/mirror-log.html)
_______

Если у вас в настройках сервера Jellyfin не открывается раздел с плагинами (такое может быть из-за блокировки ресурса на котором расположен репозиторий),
тогда можно вручную отредактировать файл Jellyfin/config/system.xml


Для этого нужно сделать следующее:

1. Остановить сервер Jellyfin.

2. Открыть файл настроек указанный выше.

3. Найти раздел отвечающий за репозиторий.

4. Вписать туда ссылку на альтернативный репозиторий.

5. Закрыть файл сохранив изменения.

6. Запустить сервер Jellyfin.

```
<PluginRepositories>
	<RepositoryInfo>
		<Name>Mirror All In One Plugins</Name>
		<Url>https://repo.jellyfin.org/files/plugin/manifest.json</Url>
		<Enabled>true</Enabled>
	</RepositoryInfo>
</PluginRepositories>
```
_______
