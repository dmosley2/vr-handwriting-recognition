import json
import time
import fastapi
from fastapi import UploadFile, File
import numpy as np
import cv2
import matplotlib.pyplot as plt
from starlette.responses import JSONResponse
from transformers import TrOCRProcessor, VisionEncoderDecoderModel
import torch
import os
import math
import sys
import uvicorn

from PIL import Image, ImageDraw, ImageFont

# MIN_LINE_GAP = 40
# MIN_WORD_GAP = 25
# MIN_BLOCK_SIZE = 30
# BUFFER = 10

WORD_CLOSING_KERNEL_WIDTH = 25
LINE_CLOSING_KERNEL_HEIGHT = 15
MIN_WORD_HEIGHT = 20
MIN_WORD_WIDTH = 20

os.environ["TRANSFORMERS_VERBOSITY"] = "error"

app = fastapi.FastAPI()
script_dir = os.path.dirname(os.path.realpath(__file__))
# local_model_path = "C:\\Users\\domin\\VirtualPainting\\VirtualPainting\\Assets\\StreamingAssets\\TrOCR"
local_model_path = script_dir + "/TrOCR"
processor = TrOCRProcessor.from_pretrained(local_model_path, local_files_only=True)
model = VisionEncoderDecoderModel.from_pretrained(local_model_path, local_files_only=True)

@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    try:
        await file.seek(0)
        image_data = await file.read()

        if not image_data:
            return JSONResponse(status_code=400,
                                content={"text": "Received empty image data", "confidence": 0.0, "time": 0.0})
        # image_path = "C:\\Users\\domin\\VirtualPainting\\VirtualPainting\\Assets\\StreamingAssets\\PreScan\\Alphabet0.png"
        nparray = np.frombuffer(image_data, np.uint8)
        image = cv2.imdecode(nparray, cv2.IMREAD_COLOR)

        if image is None:
            return JSONResponse(status_code=400,
                                content={"text": "OpenCV could not decode the image format", "confidence": 0.0,
                                         "time": 0.0})

        grey_image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        ret, binary_img = cv2.threshold(grey_image, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        img_split = []

        # Find where the text starts/ends globally
        # h_prof = np.sum(binary_img, axis=1)
        # v_prof = np.sum(binary_img, axis=0)
        #
        # y_ink = np.where(h_prof < (binary_img.shape[1] * 255) * 0.99)[0]
        # x_ink = np.where(v_prof < (binary_img.shape[0] * 255) * 0.99)[0]

        # Assuming binary_img is your 2D numpy array (White=255, Black=0)
        # Find all pixels that are NOT white
        y_coords, x_coords = np.where(binary_img < 255)

        if len(y_coords) > 0:
            y_min, y_max = np.min(y_coords), np.max(y_coords)
            x_min, x_max = np.min(x_coords), np.max(x_coords)

            # Crop to the exact box of the ink (plus a small 5px buffer)
            binary_img = binary_img[max(0, y_min - 5):min(binary_img.shape[0], y_max + 5),
            max(0, x_min - 5):min(binary_img.shape[1], x_max + 5)]

        img_split = contours_segmentation(binary_img)

        full_transcription = []

        result = ""
        resultScores = []
        total_time = 0
        if len(img_split) > 0:
            for word_img in img_split:
                image_rgb = cv2.cvtColor(word_img, cv2.COLOR_GRAY2RGB)
                start_time = time.perf_counter()
                pixel_values = processor(images=image_rgb, return_tensors="pt").pixel_values
                generated_ids = model.generate(pixel_values, output_scores=True, return_dict_in_generate=True)
                end_time = time.perf_counter()
                # generated_text = processor.batch_decode(generated_ids.sequences, skip_special_tokens=True)[0]
                score = model.compute_transition_scores(generated_ids.sequences, generated_ids.scores,
                                                        normalize_logits=True)
                tokens = generated_ids.sequences[0][1:]
                scores = score[0]
                data = zip(tokens, scores)
                listLen = len(tokens)
                for i, (token_id, score) in enumerate(data):
                    confidence = math.exp(score)
                    result += processor.decode(token_id, skip_special_tokens=True) + " "
                    resultScores.append(confidence)
                    total_time += (end_time - start_time)
        else:
            start_time = time.perf_counter()
            pixel_values = processor(images=binary_img, return_tensors="pt").pixel_values
            generated_ids = model.generate(pixel_values, output_scores=True, return_dict_in_generate=True)
            end_time = time.perf_counter()
            score = model.compute_transition_scores(generated_ids.sequences, generated_ids.scores,
                                                    normalize_logits=True)
            resultScores.append(math.exp(score))
            result = processor.decode(generated_ids.sequences[0], skip_special_tokens=True)
            total_time = end_time - start_time

        scoreAvg = 0
        if len(resultScores) > 0:
            scoreAvg = sum(resultScores) / len(resultScores)
        return {
            "text": result,
            "confidence": scoreAvg,
            "time": round(total_time, 10)
        }

    except Exception as e:
        error_message = str(e)
        return JSONResponse (status_code=500, content={
            "text": error_message,
            "confidence": 0.0,
            "time": 0.0
        })

# def recursive_xy_cut(img, direction, results, both=False):
#     h, w = img.shape
#
#     if h < MIN_BLOCK_SIZE or w < MIN_BLOCK_SIZE:
#         results.append(img)
#         return
#
#     if direction == "horizontal":
#         kernel = np.ones((15, 1), np.uint8)
#         temp_img = cv2.erode(img, kernel, iterations=1)
#         prof = np.sum(temp_img, axis=1)
#         threshold = (w * 255) * 0.8
#     else:
#         kernel = np.ones((1, 15), np.uint8)
#         temp_img = cv2.erode(img, kernel, iterations=1)
#         prof = np.sum(temp_img, axis=0)
#         threshold = (h * 255) * 0.9
#
#
#     # print(f"Shape: {img.shape}, Dir: {direction}, GapFound: {gap_width}")
#
#     gap = MIN_LINE_GAP if direction == "horizontal" else MIN_WORD_GAP
#
#     gap_start, gap_end, gap_width = find_best_split(prof, threshold, gap)
#
#     if gap_width >= gap:
#         split = (gap_start + gap_end) // 2
#
#         if direction == "horizontal":
#             part_one = img[:min(h, split), :]
#             part_two = img[max(0, split):, :]
#         else:
#             part_one = img[:, :min(h, split)]
#             part_two = img[:, max(0, split):]
#
#         next_dir = "vertical" if direction == "horizontal" else "horizontal"
#         recursive_xy_cut(part_one, next_dir, results, False)
#         recursive_xy_cut(part_two, next_dir, results, False)
#     else:
#         if not both:
#             new_dir = "vertical" if direction == "horizontal" else "horizontal"
#             recursive_xy_cut(img, new_dir, results, True)
#         else:
#             results.append(img)

#Segments handwriting image by smearing characters of words together to form a bounding box around the word
def contours_segmentation(binary_img):
    #findContours requires image with white text, input image has black text
    inverted_img = cv2.bitwise_not(binary_img)
    kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (WORD_CLOSING_KERNEL_WIDTH, 5))
    #Connects nearby white pixels to form one giant "blob" for contouring
    morphed_img = cv2.morphologyEx(inverted_img, cv2.MORPH_CLOSE, kernel)
    #Get contours and bounding boxes for each word
    contours, _ = cv2.findContours(morphed_img, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    bounding_boxes = [cv2.boundingRect(contour) for contour in contours]
    bounding_boxes = [b for b in bounding_boxes if b[2] > MIN_WORD_WIDTH and b[3] > MIN_WORD_HEIGHT]
    bounding_boxes.sort(key=lambda x: x[1])
    #Sort bounding boxes
    lines = []
    current_line_y = -1
    line_buffer = []
    if bounding_boxes:
        current_line_y = bounding_boxes[0][1]

    for x,y,w,h in bounding_boxes:
        #Word on the same line as previous word
        if abs(y - current_line_y) < LINE_CLOSING_KERNEL_HEIGHT:
            line_buffer.append((x,y,w,h))
        #Word on a new line
        else:
            line_buffer.sort(key=lambda b: b[0])
            lines.extend(line_buffer)
            line_buffer = [(x,y,w,h)]
            current_line_y = y
    line_buffer.sort(key=lambda b: b[0])
    lines.extend(line_buffer)

    word_images = []
    for x,y,w,h in lines:
        padding = 5
        y_start = max(0, y - padding)
        y_end = min(binary_img.shape[0], y + h + padding)
        x_start = max(0, x - padding)
        x_end = min(binary_img.shape[1], x + w + padding)
        cropped_word = binary_img[y_start:y_end, x_start:x_end]
        word_images.append(cropped_word)

    return word_images



def find_best_split(profile, threshold, min_gap_size):
    all_gaps = []
    start = 0
    in_gap = False

    # 1. Find every gap in the profile
    for i in range(len(profile) + 1):
        is_white = (i < len(profile)) and (profile[i] >= threshold)
        if is_white and not in_gap:
            in_gap = True
            start = i
        elif not is_white and in_gap:
            width = i - start
            if width >= min_gap_size:
                all_gaps.append((start, i, width))
            in_gap = False

    if not all_gaps:
        return 0, 0, 0

    # 2. Filter out gaps that touch the very edges (the margins)
    internal_gaps = [g for g in all_gaps if g[0] > 0 and g[1] < len(profile)]

    if not internal_gaps:
        return 0, 0, 0

    # 3. Of the remaining internal gaps, pick the one closest to the center
    center = len(profile) / 2
    best_gap = min(internal_gaps, key=lambda g: abs(((g[0] + g[1]) / 2) - center))

    return best_gap[0], best_gap[1], best_gap[2]

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=5000)

