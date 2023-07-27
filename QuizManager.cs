/*
-------------------------------------------------------------------------------------
File:         QuizManager.cs
Project:      HistoryVR
Programmer:   Dhruba Karmakar <dhrubakarmakar4321@gmail.com>
First Version:2023-07-27
-------------------------------------------------------------------------------------

Description:
This code represents a Quiz Manager for a quiz game in Unity. 
It is responsible for managing the flow of the quiz and updating the UI based on user input.

-------------------------------------------------------------------------------------

Copyright (C) 2023 DigiDrub India Pvt Ltd. All rights reserved.

Unauthorized copying of this file, via any medium is strictly prohibited.
Proprietary and confidential.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HistoryVR
{
    [System.Serializable]
    public class Question
    {
        public string question;
        public List<string> options;
        public int correctOptionIndex;
    }

    // Apply a header image for the code that only runs on Editor
    [HeaderImage("Assets/_HistoryInVR/Art/Sprites/QuizScreen.png")]

    public class QuizManager : MonoBehaviour
    {
        [ColoredHeader(1f,  1f, 1f, "List Of Question & Answer")]
        public List<Question> questions;
        [HideInInspector] public int currentQuestionIndex;

        [Space]
        [ColoredHeader(0.6f, 0.0f, 0.0f, "Quiz UI")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private List<Button> optionButtons;
        [SerializeField] private List<TextMeshProUGUI> optionTexts;
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI testResult;
        [SerializeField] TextMeshProUGUI testScore;

        [Space]
        [ColoredHeader(0.0f, 0.0f, 1.0f, "Audio Clips")]
        [SerializeField] AudioClip correctAns;
        [SerializeField] AudioClip wrongAns;

        [Space]
        [ColoredHeader(1.0f, 1.0f, .4f, "Audio Source")]
        [SerializeField] AudioSource soundEffect_audioSource;

        [Space]
        [ColoredHeader(.6f, 1.0f, .4f, "General")]
        [SerializeField] GameObject testPanel;
        [SerializeField] GameObject resultPanel;
        [SerializeField] GameObject Quiz_UI;


        private int score = 0; // Track the player's score
        private bool isWaiting = false; // Determine if the game is waiting for something

        public static QuizManager Instance; // Singleton instance of the QuizManager

        // This method is called when the game starts
        private void Start()
        {
            if(Instance == null)
                Instance = this; // Set the singleton instance if it's null

            Quiz_UI.SetActive(false); // Hide the Quiz UI initially
            scoreText.text = "Score: [ " + "0" + "/04 ]"; // Initialize score text
                    
            // LoadQuestion(questions[currentQuestionIndex]);
        }

        // This method loads a given question onto the UI
        public void LoadQuestion(Question question)
        {
            Quiz_UI.SetActive(true); // Show the Quiz UI
            isWaiting = false; // We're not waiting for anything now
            StartCoroutine(hideHeadArrowIndicator()); // Hide the head arrow indicator

            questionText.text = question.question; // Set the question text

            // Loop over each option in the question
            for (int i = 0; i < optionTexts.Count; i++)
            {
                optionButtons[i].onClick.RemoveAllListeners(); // remove previous listeners
                optionTexts[i].text = question.options[i]; // Set the option text
                int index = i;
                optionButtons[i].onClick.AddListener(() => OnOptionClicked(index)); // Add a listener to the option button
            }
        }

        // This method is called when an option is clicked
        private void OnOptionClicked(int index)
        {
            if (isWaiting) 
                return; // return if already waiting

            optionButtons[index].onClick.RemoveAllListeners(); // Remove listeners from the clicked button

            // Check if the clicked option is correct
            if (index == questions[currentQuestionIndex].correctOptionIndex)
            {
                score++; // Increase the score by +1
                scoreText.text = "Score: [ " + score.ToString() + "/04 ]"; // Visualize the score

                // Set the feedback for the user
                popupText.text = "Correct!";
                popupText.color = Color.green;
                optionButtons[index].GetComponent<Image>().color = Color.green;
                soundEffect_audioSource.PlayOneShot(correctAns); // Play the correct answer sound
                StartCoroutine(NextQuestionDelay()); // Start the delay before the next question
            }
            else
            {
                // The clicked option was incorrect
                score--; // Decrease the score by -1
                scoreText.text = "Score: [ " + score.ToString() + "/04 ]"; // Visualize the score

                // Set the feedback for the user
                popupText.text = "Try Again!";
                popupText.color = Color.red;
                optionButtons[index].GetComponent<Image>().color = Color.red;
                soundEffect_audioSource.PlayOneShot(wrongAns); // Play the wrong answer sound
            }
        }

        // This method starts a delay before the next question is shown
        private IEnumerator NextQuestionDelay()
        {
            isWaiting = true; // We're now waiting

            yield return new WaitForSeconds(2); // Wait for 2 seconds
            popupText.text = ""; // Clear the popup text

            // Reset the color of all the option buttons
            foreach(Button option in optionButtons)
            {
                option.GetComponent<Image>().color = Color.white;
            }

            currentQuestionIndex++; // Move to the next question
            // Check if there are any more questions
            if (currentQuestionIndex < questions.Count)
            {
                LoadQuestion(questions[currentQuestionIndex]); // Load the next question
            }
            else
            {
                // The quiz is finished
                testPanel.SetActive(false); // Hide the test panel
                resultPanel.SetActive(true); // Show the result panel

                // Show the test result and score based on the player's score
                if(score > 0)
                {
                    testResult.text = "Passed";
                    testResult.color = Color.green;
                    testScore.text = "Score: " + score.ToString();
                    testScore.color = Color.green;
                }
                else
                {
                    testResult.text = "Failed";
                    testResult.color = Color.red;
                    testScore.text = "Score: " + score.ToString();
                    testScore.color = Color.red;
                }

                Debug.Log("Quiz is finished"); // Log that the quiz is finished
            }
        }

        // Hide Indicator after a certain time
        IEnumerator hideHeadArrowIndicator()
        {
            yield return new WaitForSeconds(4f);
            // Hide the indicator for the quizUI
            GameManager.Instance.playerHeadArrowIndicator.SetActive(false);
            Debug.Log("Head Arrow Indicator visible");
        }
    }
}