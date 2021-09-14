import { useState } from 'react';
import { ErrorMessage } from '@hookform/error-message';
import clsx from 'clsx';
import PropTypes from 'prop-types';
import { CloudUploadIcon, XIcon, CheckIcon } from '@heroicons/react/outline';
import ErrorMessageTag from './ErrorMessage';
import { Label } from './TextInput';

export const LimitedTextarea = ({
  name,
  rows,
  placeholder,
  value,
  limit,
  onChange,
  inputRef,
  register,
  errors,
  classNames,
  disabled,
}) => {
  const [length, setLength] = useState(value?.length || 0);
  let change = onChange ? onChange : (event) => setLength(event.target.value.length);

  const classes = clsx(
    'relative',
    // 'block w-full px-3 py-2 text-sm leading-tight text-gray-800 placeholder-gray-400 transition duration-100 ease-in-out bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:outline-none focus:ring-opacity-50',
    classNames
  );
  return (
    <div className="relative flex flex-grow">
      <textarea
        name={name}
        disabled={disabled}
        id={name}
        rows={rows}
        type="textarea"
        defaultValue={value}
        ref={inputRef}
        maxLength={limit}
        placeholder={placeholder}
        className={classes}
        onChange={change}
        {...register(name)}
      ></textarea>
      {length > 0 && (
        <span className="absolute bottom-0 text-xs text-gray-400 right-4">{limit - length} characters left</span>
      )}
      <ErrorMessage errors={errors} name={name} as={ErrorMessageTag} />
    </div>
  );
};

LimitedTextarea.propTypes = {
  /**
   * The property name used by react hook form
   */
  name: PropTypes.string,
  /**
   * The help text to display
   */
  placeholder: PropTypes.string,
  /**
   * The value to preset the input to
   */
  value: PropTypes.string,
  /**
   * The number of rows to have in the textarea
   */
  rows: PropTypes.string,
  /**
   * The character count limit
   */
  limit: PropTypes.number.isRequired,
  /**
   * The ref property for use with registering with react hook form
   */
  inputRef: PropTypes.oneOfType([PropTypes.object, PropTypes.func]),
  /**
   * The function to execute when the text area value changes
   */
  onChange: PropTypes.func,
};

LimitedTextarea.defaultProps = {
  name: null,
  placeholder: null,
  value: '',
  rows: 3,
  limit: 500,
  inputRef: null,
  onChange: undefined,
  disabled: false,
};

export const LimitedTextareaFileInput = () => {
  const [file, setFile] = useState(null);
  const register = () => {};
  const formState = { errors: {} };
  const labelClasses = clsx('flex items-center bg-gray-800 rounded-r justify-center', {
    'flex-col': file,
  });

  return (
    <div className="flex flex-col">
      <Label id="injectateCharacterization" />
      <div className="flex flex-row">
        <LimitedTextarea
          name="injectateCharacterization"
          rows="5"
          limit={2500}
          register={register}
          disabled={file}
          placeholder={
            !file
              ? 'Type your characterization or upload a document'
              : 'This field is disabled when an attachment is chosen'
          }
          errors={formState.errors}
          className="border-0 border-t border-b border-r rounded-l rounded-r shadow-none"
        />
        <label className={labelClasses}>
          {file ? (
            <>
              <CheckIcon className="w-8 h-8 mx-2 text-green-500" />
              <XIcon className="w-8 h-8 mx-2 text-pink-500" />
            </>
          ) : (
            <>
              <CloudUploadIcon className="w-8 h-8 mx-2 text-white" />
              <input
                name="injectateCharacterizationFile"
                type="file"
                className="hidden"
                {...register}
                onChange={(e) => setFile(e.target.value)}
              />
            </>
          )}
        </label>
      </div>
    </div>
  );
};
