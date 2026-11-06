using System.Collections;
using UnityEngine;
using TMPro;

public class MathGame : MonoBehaviour
{
    public TMP_Text questionText;
    public TMP_Text correctText;
    public TMP_Text errorText;
    public TMP_Text feedbackText;

    public GameObject startPanel;
    public GameObject gameOverPanel;
    public TMP_Text correctResultText;
    public TMP_Text errorResultText;

    public GameObject answerCubePrefab;
    public Transform[] spawnPoints;
    public Color[] cubeColors = new Color[3];

    [Header("Game settings")]
    public int maxQuestions = 10;
    public float memoryShowSeconds = 3f;
    public float nextQuestionDelay = 1f;

    private AnswerCube[] cubes = new AnswerCube[3];
    private int correctCount;
    private int errorCount;
    private int lastCorrectIndex = -1;
    private int questionCount;
    private bool roundLocked;
    private bool gameRunning;

    private GameCategory selectedCategory = GameCategory.Mathematics;

    private enum GameCategory { Mathematics, Logic, Science, Memory }

    private class Question
    {
        public string prompt;
        public string[] answers;
        public int correctIndex;
        public string memoryPreview;

        public Question(string prompt, string a, string b, string c, int correctIndex, string memoryPreview = "")
        {
            this.prompt = prompt;
            answers = new string[] { a, b, c };
            this.correctIndex = correctIndex;
            this.memoryPreview = memoryPreview;
        }
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
    }

    // Har bir kategoriya tugmasining OnClick qismiga mos metodni ulang.
    public void StartMathematics() { SelectCategory(GameCategory.Mathematics); }
    public void StartLogic()       { SelectCategory(GameCategory.Logic); }
    public void StartScience()     { SelectCategory(GameCategory.Science); }
    public void StartMemory()      { SelectCategory(GameCategory.Memory); }

    private void SelectCategory(GameCategory category)
    {
        selectedCategory = category;
        StartGame();
    }

    public void StartGame()
    {
        StopAllCoroutines();
        ClearCubes();

        questionCount = 0;
        correctCount = 0;
        errorCount = 0;
        lastCorrectIndex = -1;
        roundLocked = false;
        gameRunning = true;

        if (correctText != null) correctText.text = "Correct: 0";
        if (errorText != null) errorText.text = "Error: 0";
        if (feedbackText != null) feedbackText.text = "";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(false);

        NewQuestion();
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void QuitGame()
    {
        StopAllCoroutines();
        gameRunning = false;
        roundLocked = false;
        ClearCubes();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
        if (questionText != null) questionText.text = "";
        if (feedbackText != null) feedbackText.text = "";
    }

    private void NewQuestion()
    {
        if (!gameRunning) return;

        questionCount++;
        Question q = GetQuestion();

        int correctIndex = Random.Range(0, 3);
        while (correctIndex == lastCorrectIndex)
            correctIndex = Random.Range(0, 3);
        lastCorrectIndex = correctIndex;

        // Javoblarni aralashtiramiz, to'g'ri javob tasodifiy yo'lakka tushadi.
        string[] shuffledAnswers = new string[3];
        int sourceIndex = 0;
        for (int i = 0; i < 3; i++)
        {
            if (i == correctIndex)
                shuffledAnswers[i] = q.answers[q.correctIndex];
            else
            {
                while (sourceIndex == q.correctIndex) sourceIndex++;
                shuffledAnswers[i] = q.answers[sourceIndex++];
            }
        }

        if (selectedCategory == GameCategory.Memory && !string.IsNullOrEmpty(q.memoryPreview))
            StartCoroutine(ShowMemoryThenSpawn(q, shuffledAnswers, correctIndex));
        else
        {
            if (questionText != null) questionText.text = q.prompt;
            SpawnCubes(shuffledAnswers, correctIndex);
        }
    }

    private IEnumerator ShowMemoryThenSpawn(Question q, string[] answers, int correctIndex)
    {
        if (questionText != null)
            questionText.text = "Remember this:\n" + q.memoryPreview;

        yield return new WaitForSeconds(memoryShowSeconds);

        if (!gameRunning) yield break;

        if (questionText != null) questionText.text = q.prompt;
        SpawnCubes(answers, correctIndex);
    }

    private void SpawnCubes(string[] answers, int correctIndex)
    {
        if (answerCubePrefab == null || spawnPoints == null || spawnPoints.Length < 3)
        {
            Debug.LogError("MathGame: Answer Cube Prefab va 3 ta Spawn Point kerak.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject newCube = Instantiate(answerCubePrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            AnswerCube answerCube = newCube.GetComponent<AnswerCube>();

            if (answerCube == null)
            {
                Debug.LogError("Answer Cube Prefab ichida AnswerCube komponenti yo'q.");
                Destroy(newCube);
                continue;
            }

            cubes[i] = answerCube;
            answerCube.SetAnswer(answers[i], i == correctIndex);
            if (cubeColors != null && cubeColors.Length > i)
                answerCube.SetColor(cubeColors[i]);
        }
    }

    public void CheckAnswer(bool correct)
    {
        if (!gameRunning || roundLocked) return;

        roundLocked = true;

        if (correct)
        {
            correctCount++;
            if (correctText != null) correctText.text = "Correct: " + correctCount;
            if (feedbackText != null) feedbackText.text = "Correct!";
        }
        else
        {
            errorCount++;
            if (errorText != null) errorText.text = "Error: " + errorCount;
            if (feedbackText != null) feedbackText.text = "Error!";
        }

        StartCoroutine(NewRound());
    }

    private IEnumerator NewRound()
    {
        yield return new WaitForSeconds(nextQuestionDelay);

        if (feedbackText != null) feedbackText.text = "";
        ClearCubes();

        if (questionCount >= maxQuestions)
        {
            GameOver();
            yield break;
        }

        roundLocked = false;
        NewQuestion();
    }

    private void GameOver()
    {
        gameRunning = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (correctResultText != null) correctResultText.text = "Correct: " + correctCount;
        if (errorResultText != null) errorResultText.text = "Error: " + errorCount;
    }

    private void ClearCubes()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            if (cubes[i] != null)
                Destroy(cubes[i].gameObject);
            cubes[i] = null;
        }
    }

    private Question GetQuestion()
    {
        switch (selectedCategory)
        {
            case GameCategory.Logic: return GetLogicQuestion();
            case GameCategory.Science: return GetScienceQuestion();
            case GameCategory.Memory: return GetMemoryQuestion();
            default: return GetMathQuestion();
        }
    }

    private Question GetMathQuestion()
    {
        int a = Random.Range(1, 20);
        int b = Random.Range(1, 20);
        int answer = a + b;
        int wrong1 = answer + Random.Range(1, 6);
        int wrong2 = answer - Random.Range(1, 6);
        if (wrong2 < 0) wrong2 = answer + 7;
        return new Question(a + " + " + b + " = ?", answer.ToString(), wrong1.ToString(), wrong2.ToString(), 0);
    }

    private Question GetLogicQuestion()
{
    Question[] questions = new Question[]
    {
        new Question("Which number comes next?\n2, 4, 6, 8, ?", "9", "10", "12", 1),
        new Question("Which one is different?", "Apple", "Carrot", "Banana", 1),
        new Question("You have 3 apples and take away 1. How many do you have?", "1", "2", "3", 0),
        new Question("Which number comes next?\n5, 10, 15, 20, ?", "25", "30", "22", 0),
        new Question("How many days are in 2 weeks?", "12", "14", "16", 1),
        new Question("Which shape has 3 sides?", "Square", "Triangle", "Circle", 1),
        new Question("Which is the smallest number?", "0", "1", "2", 0),
        new Question("If today is Monday, what day is tomorrow?", "Sunday", "Tuesday", "Wednesday", 1),
        new Question("Which number comes next?\n1, 3, 5, 7, ?", "8", "9", "10", 1),
        new Question("Which one does not belong?", "Dog", "Cat", "Car", 2),
        new Question("How many months have 28 days?", "1", "12", "6", 1),
        new Question("If 2 cats catch 2 mice in 2 minutes, how many mice can 4 cats catch in 2 minutes?", "2", "4", "8", 1),
        new Question("Which number is missing?\n10, 20, ?, 40", "25", "30", "35", 1),
        new Question("Which is heavier?", "1 kg iron", "1 kg cotton", "They weigh the same", 2),
        new Question("A farmer has 5 sheep. All but 2 run away. How many remain?", "2", "3", "5", 0),
        new Question("Which number comes next?\n3, 6, 9, 12, ?", "14", "15", "18", 1),
        new Question("How many corners does a rectangle have?", "3", "4", "5", 1),
        new Question("If you face north and turn right, which direction are you facing?", "West", "South", "East", 2),
        new Question("Which number is even?", "7", "11", "14", 2),
        new Question("There are 10 birds. 3 fly away. How many remain?", "7", "8", "6", 0)
    };

    return questions[Random.Range(0, questions.Length)];
}

    private Question GetScienceQuestion()
{
    Question[] questions = new Question[]
    {
        new Question("Which planet do we live on?", "Mars", "Earth", "Venus", 1),
        new Question("What do plants need to make food?", "Sunlight", "Plastic", "Sand only", 0),
        new Question("Which gas do humans need to breathe?", "Oxygen", "Helium", "Hydrogen", 0),
        new Question("What is the boiling point of water at sea level?", "50 C", "100 C", "150 C", 1),
        new Question("Which part of the body pumps blood?", "Lungs", "Brain", "Heart", 2),
        new Question("Which of these is a mammal?", "Shark", "Dolphin", "Trout", 1),
        new Question("What is Earth's natural satellite?", "The Moon", "The Sun", "Mars", 0),
        new Question("What force pulls objects toward Earth?", "Magnetism", "Gravity", "Friction", 1),
        new Question("Which planet is closest to the Sun?", "Mercury", "Earth", "Jupiter", 0),
        new Question("What do bees collect from flowers?", "Water", "Nectar", "Sand", 1),
        new Question("Which organ helps us breathe?", "Stomach", "Lungs", "Kidneys", 1),
        new Question("What is the chemical formula for water?", "CO2", "H2O", "O2", 1),
        new Question("Which animal is known as the king of the jungle?", "Elephant", "Lion", "Tiger", 1),
        new Question("What do humans use to see?", "Ears", "Eyes", "Hands", 1),
        new Question("Which state of matter has a fixed shape?", "Liquid", "Gas", "Solid", 2),
        new Question("What is the center of our solar system?", "Earth", "The Sun", "The Moon", 1),
        new Question("Which vitamin can our body produce with sunlight?", "Vitamin A", "Vitamin C", "Vitamin D", 2),
        new Question("What is the largest planet in our solar system?", "Earth", "Jupiter", "Mars", 1),
        new Question("Which animal lays eggs?", "Dog", "Chicken", "Cow", 1),
        new Question("What is the main gas in Earth's atmosphere?", "Oxygen", "Nitrogen", "Carbon dioxide", 1)
    };

    return questions[Random.Range(0, questions.Length)];
}

    private Question GetMemoryQuestion()
{
    Question[] questions = new Question[]
    {
        new Question("Which sequence did you see?", "RED - BLUE - GREEN", "RED - GREEN - BLUE", "BLUE - RED - GREEN", 0, "RED - BLUE - GREEN"),

        new Question("Which sequence did you see?", "2 - 5 - 8", "2 - 8 - 5", "5 - 2 - 8", 0, "2 - 5 - 8"),

        new Question("Which sequence did you see?", "SUN - STAR - MOON", "STAR - SUN - MOON", "SUN - MOON - STAR", 0, "SUN - STAR - MOON"),

        new Question("Which sequence did you see?", "4 - 1 - 7 - 3", "4 - 7 - 1 - 3", "1 - 4 - 7 - 3", 0, "4 - 1 - 7 - 3"),

        new Question("Which sequence did you see?", "CAT - DOG - BIRD", "DOG - CAT - BIRD", "CAT - BIRD - DOG", 0, "CAT - DOG - BIRD"),

        new Question("Which sequence did you see?", "GREEN - YELLOW - RED", "YELLOW - GREEN - RED", "GREEN - RED - YELLOW", 0, "GREEN - YELLOW - RED"),

        new Question("Which sequence did you see?", "1 - 3 - 5 - 7", "1 - 5 - 3 - 7", "3 - 1 - 5 - 7", 0, "1 - 3 - 5 - 7"),

        new Question("Which sequence did you see?", "APPLE - ORANGE - GRAPE", "ORANGE - APPLE - GRAPE", "APPLE - GRAPE - ORANGE", 0, "APPLE - ORANGE - GRAPE"),

        new Question("Which sequence did you see?", "BLUE - RED - YELLOW", "RED - BLUE - YELLOW", "BLUE - YELLOW - RED", 0, "BLUE - RED - YELLOW"),

        new Question("Which sequence did you see?", "8 - 2 - 6 - 4", "8 - 6 - 2 - 4", "2 - 8 - 6 - 4", 0, "8 - 2 - 6 - 4"),

        new Question("Which sequence did you see?", "MONDAY - TUESDAY - FRIDAY", "TUESDAY - MONDAY - FRIDAY", "MONDAY - FRIDAY - TUESDAY", 0, "MONDAY - TUESDAY - FRIDAY"),

        new Question("Which sequence did you see?", "CIRCLE - SQUARE - TRIANGLE", "SQUARE - CIRCLE - TRIANGLE", "CIRCLE - TRIANGLE - SQUARE", 0, "CIRCLE - SQUARE - TRIANGLE"),

        new Question("Which sequence did you see?", "9 - 4 - 2 - 6", "9 - 2 - 4 - 6", "4 - 9 - 2 - 6", 0, "9 - 4 - 2 - 6"),

        new Question("Which sequence did you see?", "PINK - BLACK - WHITE", "BLACK - PINK - WHITE", "PINK - WHITE - BLACK", 0, "PINK - BLACK - WHITE"),

        new Question("Which sequence did you see?", "CAR - BUS - TRAIN", "BUS - CAR - TRAIN", "CAR - TRAIN - BUS", 0, "CAR - BUS - TRAIN"),

        new Question("Which sequence did you see?", "5 - 7 - 1 - 9", "5 - 1 - 7 - 9", "7 - 5 - 1 - 9", 0, "5 - 7 - 1 - 9"),

        new Question("Which sequence did you see?", "LION - TIGER - BEAR", "TIGER - LION - BEAR", "LION - BEAR - TIGER", 0, "LION - TIGER - BEAR"),

        new Question("Which sequence did you see?", "RED - WHITE - BLACK - BLUE", "WHITE - RED - BLACK - BLUE", "RED - BLACK - WHITE - BLUE", 0, "RED - WHITE - BLACK - BLUE"),

        new Question("Which sequence did you see?", "2 - 4 - 8 - 6", "2 - 8 - 4 - 6", "4 - 2 - 8 - 6", 0, "2 - 4 - 8 - 6"),

        new Question("Which sequence did you see?", "MORNING - AFTERNOON - NIGHT", "AFTERNOON - MORNING - NIGHT", "MORNING - NIGHT - AFTERNOON", 0, "MORNING - AFTERNOON - NIGHT")
    };

    return questions[Random.Range(0, questions.Length)];
}
}
