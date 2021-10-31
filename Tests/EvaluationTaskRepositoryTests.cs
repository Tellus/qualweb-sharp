using Inqlude.Database;
using System.Collections.Generic;
using Xunit;

namespace InqludeTests.Database.Repositories {
  public class EvaluationTaskRepositoryTest : BaseDatabaseTest {
    readonly EvaluationTaskRepository repo;

    EvaluationTask fullTaskFixture = new EvaluationTask() {
      CompletedUrls = 50,
      FailedUrls = 3,
      CreationDate = new System.DateTime(2020, 10, 9),
      CompletionDate = new System.DateTime(2020, 10, 17),
      State = EvaluationTaskState.Completed,
      CrawlUrl = "https://www.inqludeit.dk",
      QualwebVersion = "0.6.9",
      ConformanceLevels = new List<string>() { "A", "AA" },
    };

    public EvaluationTaskRepositoryTest() {
      ConfigureDefaults();

      repo = new EvaluationTaskRepository();
    }

    EvaluationTask CloneTask(EvaluationTask oldTask) {
      return new EvaluationTask() {
        CompletedUrls = oldTask.CompletedUrls,
        CompletionDate = oldTask.CompletionDate,
        ConformanceLevels = oldTask.ConformanceLevels,
        CrawlUrl = oldTask.CrawlUrl,
        CreationDate = oldTask.CreationDate,
        CustomerCase = oldTask.CustomerCase,
        FailedUrls = oldTask.FailedUrls,
        QualwebModules = oldTask.QualwebModules,
        QualwebVersion = oldTask.QualwebVersion,
        State = oldTask.State,
      };
    }

    [Fact]
    public async void CreateOne() {
      var result = await repo.CreateOne(this.CloneTask(fullTaskFixture));

      Assert.IsType<string>(result.Id);
      Assert.NotEqual(result.Id, fullTaskFixture.Id);
      Assert.Equal(result.CompletionDate, fullTaskFixture.CompletionDate);
      Assert.Equal(result.ConformanceLevels, fullTaskFixture.ConformanceLevels);
      Assert.Equal(result.CrawlUrl, fullTaskFixture.CrawlUrl);
      Assert.Equal(result.CreationDate, fullTaskFixture.CreationDate);
      Assert.Equal(result.CustomerCase, fullTaskFixture.CustomerCase);
      Assert.Equal(result.FailedUrls, fullTaskFixture.FailedUrls);
      Assert.Equal(result.QualwebModules, fullTaskFixture.QualwebModules);
      Assert.Equal(result.QualwebVersion, fullTaskFixture.QualwebVersion);
      Assert.Equal(result.State, fullTaskFixture.State);
    }
  }
}