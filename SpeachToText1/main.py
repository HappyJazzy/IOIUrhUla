import string
import azure.cognitiveservices.speech as speechsdk
import socket
import openai


def setup_speech_service():
    speech_key = "f286112b321247348cd06a760d2e59f7"
    service_region = "eastus"

    speech_config = speechsdk.SpeechConfig(subscription=speech_key, region=service_region)
    speech_config.speech_recognition_language = "sl-SI"

    audio_config = speechsdk.audio.AudioConfig(use_default_microphone=True)
    speech_recognizer = speechsdk.SpeechRecognizer(speech_config=speech_config, audio_config=audio_config)

    return speech_recognizer


def process_commands(speech_recognizer, client, conn):
    interaction_mode = False  # Define outside the loop
    print("Speak a command (gor, dol, levo, desno, stop, živijo, adijo).")

    while True:
        try:
            speech_recognition_result = speech_recognizer.recognize_once()

            if speech_recognition_result.reason == speechsdk.ResultReason.RecognizedSpeech:
                command = speech_recognition_result.text.lower().translate(str.maketrans('', '', string.punctuation))
                print(f"Recognized command: {command}")

                if command == "živijo":
                    interaction_mode = True
                    conn.sendall("živijo".encode())

                elif command == "adijo":
                    interaction_mode = False
                    conn.sendall("adijo".encode())

                elif interaction_mode:
                    response = get_response_from_gpt(client, command)
                    print(response)
                    conn.sendall(response.encode())

                else:
                    if command in ["gor", "dol", "levo", "desno", "stop"]:
                        conn.sendall(command.encode())

            elif speech_recognition_result.reason in [speechsdk.ResultReason.NoMatch, speechsdk.ResultReason.Canceled]:
                print("Did not recognize speech. Please try again.")

            elif speech_recognition_result.reason == speechsdk.ResultReason.Canceled:
                cancellation_details = speech_recognition_result.cancellation_details
                print(f"Speech Recognition canceled: {cancellation_details.reason}")
                break

        except Exception as e:
            print(f"An error occurred: {e}")
            break

    conn.close()

def handle_command(command, client):
    if command in ["gor", "dol", "levo", "desno", "stop"]:
        if command == "gor":
            print("Moving up!")
        elif command == "dol":
            print("Moving down!")
        elif command == "levo":
            print("Moving left!")
        elif command == "desno":
            print("Moving right!")
        elif command == "stop":
            print("Stopping!")
            exit()
    else:
        hello_index = command.find("živijo")
        if hello_index != -1:
            prompt = command[hello_index + len("živijo"):].strip()
            print(prompt)
            print(get_response_from_gpt(client, prompt))
        else:
            print("Unknown command. Please try again.")

def setup_openai_client():
    return openai.OpenAI(api_key='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')

def get_response_from_gpt(client, prompt):
    try:
        response = client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[{"role": "user", "content": prompt}]
        )
        if response.choices:
            message_content = response.choices[0].message.content
            return message_content.strip()[:60] if message_content else "No response"
        else:
            return "No response"
    except Exception as e:
        return str(e)


def start_server():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('localhost', 8080))
        s.listen()
        conn, addr = s.accept()
        with conn:
            print('Connected by', addr)
            client = setup_openai_client()
            recognizer = setup_speech_service()
            process_commands(recognizer, client, conn)

if __name__ == "__main__":
    start_server()
