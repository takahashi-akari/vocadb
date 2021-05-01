import RepositoryFactory from '../Repositories/RepositoryFactory';
import functions from '../Shared/GlobalFunctions';
import ui from '../Shared/MessagesTyped';
import UrlMapper from '../Shared/UrlMapper';
import { IEntryReportType } from '../ViewModels/ReportEntryViewModel';
import SongDetailsViewModel, {
  SongDetailsAjax,
  SongDetailsResources,
} from '../ViewModels/Song/SongDetailsViewModel';

function initPage(jsonModel, songId, saveStr, urlMapper, viewModel) {
  function initMediaPlayer() {
    $('audio').mediaelementplayer({
      pluginPath: 'https://cdnjs.com/libraries/mediaelement/',
    });
  }

  $('.js-ratingButtons').buttonset();
  $('#reportEntryLink').button({ icons: { primary: 'ui-icon-alert' } });
  $('#manageTags').button({ icons: { primary: 'ui-icon-wrench' } });
  $('#viewVersions').button({ icons: { primary: 'ui-icon-clock' } });
  $('#viewCommentsLink').click(function () {
    var index = $('#tabs ul [data-tab="Discussion"]').index();
    $('#tabs').tabs('option', 'active', index);
    return false;
  });
  $('#viewRelatedLink').click(function () {
    var index = $('#tabs ul [data-tab="Related"]').index();
    $('#tabs').tabs('option', 'active', index);
    return false;
  });

  $('#tabs').tabs({
    load: function (event, ui) {
      vdb.functions.disableTabReload(ui.tab);
    },
    activate: function (event, ui) {
      if (ui.newTab.data('tab') === 'Discussion') {
        viewModel.comments.initComments();
      }
    },
  });

  $('#pvLoader')
    .ajaxStart(function () {
      $(this).show();
    })
    .ajaxStop(function () {
      $(this).hide();
    });

  $('.pvLink:not(.disabled)').click(function () {
    var id = functions.getId(this);
    $.post(
      urlMapper.mapRelative('/Song/PVForSong'),
      { pvId: id },
      function (content) {
        $('#pvPlayer').html(content);
        initMediaPlayer();
      },
    );

    return false;
  });

  $('td.artistList a').vdbArtistToolTip();
  $('#albumList a').vdbAlbumWithCoverToolTip();
  initMediaPlayer();
}

const SongDetails = (
  canDeleteAllComments: boolean,
  model: {
    id: number;
    jsonModel: SongDetailsAjax;
  },
  reportTypes: IEntryReportType[],
  resources: SongDetailsResources,
  saveStr: string,
  showTranslatedDescription: boolean,
) => {
  $(document).ready(function () {
    moment.locale(vdb.values.culture);

    vdb.resources.song = resources;

    var jsonModel = model.jsonModel;
    var rootPath = vdb.values.baseAddress;
    var urlMapper = new UrlMapper(rootPath);
    var repoFactory = new RepositoryFactory(
      urlMapper,
      vdb.values.languagePreference,
    );
    var repo = repoFactory.songRepository();
    var userRepo = repoFactory.userRepository();
    var artistRepo = repoFactory.artistRepository();

    var viewModel = new SongDetailsViewModel(
      repo,
      userRepo,
      artistRepo,
      resources,
      showTranslatedDescription,
      jsonModel,
      reportTypes,
      vdb.values.loggedUserId,
      vdb.values.languagePreference,
      canDeleteAllComments,
      ui.showThankYouForRatingMessage,
    );
    ko.applyBindings(viewModel);

    viewModel.songListDialog.addedToList = function () {
      ui.showSuccessMessage(resources.addedToList);
    };

    initPage(jsonModel, model.id, saveStr, urlMapper, viewModel);
  });
};

export default SongDetails;
