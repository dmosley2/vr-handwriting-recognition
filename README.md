# VR Handwriting Recognition

This project explores using VR as a platform for handwriting practice, combining it with AI to give users real-time feedback on what they write. Users can write using three different interfaces — a collision-based controller, a raycast-based controller, and a haptic device — with a TrOCR model running in the background to recognize their handwriting.

The project was completed as an M.S. thesis at Eastern Washington University. An earlier version of the work, covering model selection and an initial OpenCV-based interface, was published at [IEEE AIxVR 2026](https://ieeexplore.ieee.org/abstract/document/11450025).
## Technical Highlights

- Embedded Python runtime within Unity to run inference without an external server
- FastAPI inference server bridging Unity and the TrOCR model
- TrOCR integration for handwritten text recognition
- Three distinct VR writing interfaces, each with different interaction models and rendering approaches
- 19-participant user study with statistical analysis using Linear Mixed Effects Regression (LMER)
- Color-coded confidence feedback system for real-time model output visualization

## Tech Stack

- **Unity** — VR application and interface development
- **C#** — application logic and VR interaction scripts
- **Python** — embedded runtime for running the inference server
- **TrOCR** — transformer-based handwritten text recognition model
- **FastAPI** — inference server bridging Unity and the TrOCR model
- **OpenCV** — used in early prototype for image preprocessing

## Setup

### Hardware
- Meta Quest headset
- 3D Systems Touch Actor (for haptic device interface)

### Unity Assets
The following third-party assets are required but not included in this repo. Import them from the Unity Asset Store:
- Furniture Mega Pack
- Office Pack
- Office Supplies Low Poly
- Picture Frames FREE

### Model Weights
The TrOCR model weights are not included due to file size. Download them and place them in `Assets/StreamingAssets/TrOCR/`:
- `pytorch_model.bin`
- `model.safetensors`
- `encoder_model.onnx`
- `decoder_model_merged.onnx`
- `tokenizer.json`

The models can be downloaded from [HuggingFace](https://huggingface.co/microsoft/trocr-base-handwritten).

### Python Dependencies
The embedded Python runtime requires the following packages:
- torch 2.10.0 (CUDA 12.6)
- torchvision
- transformers
- fastapi
- uvicorn
- opencv-python
- pillow
- numpy
- huggingface-hub
- safetensors
- pydantic
- httpx

## Demo

**Collision Interface**


[![Collision Interface Demo](http://img.youtube.com/vi/JvpwV6-dvho/0.jpg)](https://www.youtube.com/watch?v=JvpwV6-dvho)

**Raycast Interface**


[![Raycast Interface Demo](http://img.youtube.com/vi/-Z_xgBTXJOg/0.jpg)](https://www.youtube.com/watch?v=-Z_xgBTXJOg)

**Haptic Interface**


[![Haptic Interface Demo](http://img.youtube.com/vi/aJeKQRdY2Qk/0.jpg)](https://www.youtube.com/watch?v=aJeKQRdY2Qk)

## Study Findings

A user study with 19 participants was conducted to evaluate the three interfaces across objective and subjective measures.

- The **haptic interface** produced the highest model confidence and lowest character error rates, and was rated the most usable by participants.
- The **collision interface** performed moderately across both objective and subjective measures.
- The **raycast interface** performed worst on both model confidence and usability ratings.

Overall the results suggest that physically grounded interfaces are better suited for handwriting in VR than raycast-based input.
