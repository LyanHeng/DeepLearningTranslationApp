from multiprocessing import current_process
from unittest import result
from nltk.translate import bleu
import pandas as pd
import statistics as statistics


def main():
    filename = r'dataset.xlsx'
    sheetname = r'dataset'
    number_of_mrs = 7

    # retrieve data by MR group
    data = get_data_by_mr(
        filename=filename, sheetname=sheetname, number_of_mrs=number_of_mrs)

    # calcualate BLEU score for each MR group
    result_by_mr = {}
    i = 1
    for each_data in data:
        result_by_mr.update({f"MR{i}": calculate_MR_bleu_score(
            each_data['Followup'], each_data['Result'])})
        i = i+1

    # calculate average of each group and print average score
    for key in result_by_mr:
        average_score = statistics.mean(result_by_mr[key])
        print(f"Average BLEU score of {key}: {str(average_score)}")


# retrieve data from file separating them into MR groups
def get_data_by_mr(filename, sheetname, number_of_mrs=0):
    df = pd.read_excel(filename, sheetname)
    data_by_mr = []

    # get data by MR
    for i in range(number_of_mrs):
        # get all MR data without blank (-) test cases (i.e. MR1)
        mask = (df['MR'] == i+1) & (df['Followup'] != "-")
        current_mr_data = df.loc[mask]
        # append data to current_mr_data
        data_by_mr.append(current_mr_data)

    return data_by_mr


# separate each bleu score by MRs
def calculate_MR_bleu_score(followups, results):
    if isinstance(followups, pd.Series):
        followups = followups.array
    if isinstance(results, pd.Series):
        results = results.array

    bleu_scores = []

    # check length of the comparison lists
    if (followups.size != results.size):
        return -1

    for i in range(followups.size-1):
        bleu_scores.append(bleu_score(
            reference=followups[i], candidate=results[i]))

    return bleu_scores


# calculate bleu score of each sentence pair
def bleu_score(reference, candidate):
    return bleu(reference, candidate, (2,))


if __name__ == '__main__':
    main()
