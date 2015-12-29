﻿using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VocaDb.Model.Database.Queries;
using VocaDb.Model.DataContracts;
using VocaDb.Model.DataContracts.Tags;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Domain.Images;
using VocaDb.Model.Domain.Tags;
using VocaDb.Model.Domain.Users;
using VocaDb.Model.Service.Exceptions;
using VocaDb.Tests.TestData;
using VocaDb.Tests.TestSupport;

namespace VocaDb.Tests.Web.Controllers.DataAccess {

	/// <summary>
	/// Tests for <see cref="TagQueries"/>.
	/// </summary>
	[TestClass]
	public class TagQueriesTests {

		private InMemoryImagePersister imagePersister;
		private FakePermissionContext permissionContext;
		private TagQueries queries;
		private FakeTagRepository repository;
		private Tag tag;
		private Tag tag2;
		private User user;

		private ArchivedTagVersion GetArchivedVersion(Tag tag) {
			return repository.List<ArchivedTagVersion>().FirstOrDefault(a => a.Tag.Id == tag.Id);
		}

		private Tag CreateAndSaveTag(string englishName) {

			var t = CreateEntry.Tag(englishName);

			repository.Save(t);

			foreach (var name in t.Names)
				repository.Save(name);

			return t;

		}

		private Stream TestImage() {
			return ResourceHelper.TestImage();
		}

		[TestInitialize]
		public void SetUp() {

			repository = new FakeTagRepository();

			tag = CreateAndSaveTag("Appearance_Miku");
			tag2 = CreateAndSaveTag("MMD");

			user = new User("User", "123", "test@test.com", 123);
			repository.Add(user);

			permissionContext = new FakePermissionContext(new UserWithPermissionsContract(user, ContentLanguagePreference.Default));

			imagePersister = new InMemoryImagePersister();
			queries = new TagQueries(repository, permissionContext, new FakeEntryLinkFactory(), imagePersister, new FakeUserIconFactory());

		}

		[TestMethod]
		public void GetTagsByCategories() {
			
			tag.CategoryName = "Animation";

			var result = queries.GetTagsByCategories();

			Assert.AreEqual(2, result.Length, "Number of categories");
			var firstCategory = result[0];
			Assert.AreEqual("Animation", firstCategory.Name, "First category name");
			Assert.AreEqual(1, firstCategory.Tags.Length, "Number of tags in the Animation category");
			Assert.AreEqual("Appearance_Miku", firstCategory.Tags[0].Name, "First tag in the Animation category");

		}

		[TestMethod]
		public void Update_Description() {

			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			updated.Description = "mikumikudance.wikia.com/wiki/Miku_Hatsune_Appearance_(Mamama)";

			queries.Update(updated, null);

			Assert.AreEqual(updated.Description, tag.Description, "Description was updated");

			var archivedVersion = repository.List<ArchivedTagVersion>().FirstOrDefault(a => a.Tag.Id == tag.Id);
			Assert.IsNotNull(archivedVersion, "Archived version was created");
			Assert.AreEqual(TagEditableFields.Description, archivedVersion.Diff.ChangedFields, "Changed fields");

		}

		[TestMethod]
		public void Update_Image() {
			
			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			using (var stream = TestImage()) {
				queries.Update(updated, new UploadedFileContract { Mime = MediaTypeNames.Image.Jpeg, Stream = stream });			
			}

			var thumb = new EntryThumb(tag, MediaTypeNames.Image.Jpeg);
			Assert.IsTrue(imagePersister.HasImage(thumb, ImageSize.Original), "Original image was saved");
			Assert.IsTrue(imagePersister.HasImage(thumb, ImageSize.SmallThumb), "Small thumbnail was saved");

			var archivedVersion = GetArchivedVersion(tag);
			Assert.IsNotNull(archivedVersion, "Archived version was created");
			Assert.AreEqual(TagEditableFields.Picture, archivedVersion.Diff.ChangedFields, "Changed fields");

		}

		[TestMethod]
		public void Update_Name() {

			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			updated.Names[0].Value = "Api_Miku";

			queries.Update(updated, null);

			Assert.AreEqual("Api_Miku", tag.DefaultName, "EnglishName");

			var archivedVersion = GetArchivedVersion(tag);
			Assert.IsNotNull(archivedVersion, "Archived version was created");
			Assert.AreEqual(TagEditableFields.Names, archivedVersion.Diff.ChangedFields, "Changed fields");

		}

		[TestMethod]
		[ExpectedException(typeof(DuplicateTagNameException))]
		public void Update_Name_Duplicate() {

			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			updated.Names[0].Value = "MMD";

			queries.Update(updated, null);

		}

		[TestMethod]
		public void Update_Parent() {
			
			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			updated.Parent = new TagBaseContract(tag2, ContentLanguagePreference.English);

			queries.Update(updated, null);

			Assert.AreEqual(tag2, tag.Parent, "Parent");
			Assert.IsTrue(tag2.Children.Contains(tag), "Parent contains child tag");

			var archivedVersion = GetArchivedVersion(tag);
			Assert.IsNotNull(archivedVersion, "Archived version was created");
			Assert.AreEqual(TagEditableFields.Parent, archivedVersion.Diff.ChangedFields, "Changed fields");

		}

		[TestMethod]
		public void Update_Parent_IgnoreSelf() {
			
			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			updated.Parent = new TagBaseContract(tag, ContentLanguagePreference.English);

			queries.Update(updated, null);

			Assert.IsNull(tag.Parent, "Parent");

			var archivedVersion = GetArchivedVersion(tag);
			Assert.IsNotNull(archivedVersion, "Archived version was created");
			Assert.AreEqual(TagEditableFields.Nothing, archivedVersion.Diff.ChangedFields, "Changed fields");

		}

		[TestMethod]
		public void Update_Parent_Renamed() {

			var updated = new TagForEditContract(tag, false, ContentLanguagePreference.English);
			tag2.TranslatedName.Default = "Api_Miku";
			updated.Parent = new TagBaseContract(tag2, ContentLanguagePreference.English);

			queries.Update(updated, null);

			Assert.AreEqual(tag2, tag.Parent, "Parent");
			Assert.IsTrue(tag2.Children.Contains(tag), "Parent contains child tag");

		}

	}
}
